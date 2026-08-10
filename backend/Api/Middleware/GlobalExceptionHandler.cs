using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CBRE.TaskListDemo.Api.Middleware
{
	public class GlobalExceptionHandler : IExceptionHandler
	{
		private readonly ILogger<GlobalExceptionHandler> _logger;
		private readonly IHostEnvironment _environment;
		private readonly IProblemDetailsService _problemDetailsService;

		public GlobalExceptionHandler(
			ILogger<GlobalExceptionHandler> logger,
			IHostEnvironment environment,
			IProblemDetailsService problemDetailsService)
		{
			_logger = logger;
			_environment = environment;
			_problemDetailsService = problemDetailsService;
		}

		public async ValueTask<bool> TryHandleAsync(
			HttpContext httpContext,
			Exception exception,
			CancellationToken cancellationToken)
		{
			var (statusCode, title, detail) = MapException(exception);

			_logger.LogError(
				exception,
				"Unhandled exception of type {ExceptionType} occurred while processing {Method} {Path}. Mapped to {StatusCode}.",
				exception.GetType().Name,
				httpContext.Request.Method,
				httpContext.Request.Path,
				statusCode);

			httpContext.Response.StatusCode = statusCode;

			var problemDetails = new ProblemDetails
			{
				Status = statusCode,
				Title = title,
				Detail = detail,
				Instance = httpContext.Request.Path
			};

			if (_environment.IsDevelopment())
			{
				problemDetails.Extensions["exception"] = exception.GetType().FullName;
				problemDetails.Extensions["stackTrace"] = exception.StackTrace;
			}

			return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
			{
				HttpContext = httpContext,
				Exception = exception,
				ProblemDetails = problemDetails
			});
		}

		private static (int StatusCode, string Title, string Detail) MapException(Exception exception) =>
			exception switch
			{
				DbUpdateConcurrencyException => (
					StatusCodes.Status409Conflict,
					"Concurrency conflict",
					"The record you attempted to modify was changed or deleted by another request. Please reload and try again."),

				DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) => (
					StatusCodes.Status409Conflict,
					"Duplicate resource",
					"A resource with the same unique value already exists."),

				DbUpdateException => (
					StatusCodes.Status400BadRequest,
					"Database update failed",
					"The request could not be completed due to a database constraint violation."),

				_ => (
					StatusCodes.Status500InternalServerError,
					"An unexpected error occurred",
					"An unexpected error occurred while processing your request.")
			};

		private static bool IsUniqueConstraintViolation(DbUpdateException dbEx) =>
			dbEx.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
	}
}
