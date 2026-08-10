using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CBRE.TaskListDemo.Infrastructure.Services
{
	public class FakeEmailSender : IEmailSender
	{
		private readonly ILogger<FakeEmailSender> _logger;

		public FakeEmailSender(ILogger<FakeEmailSender> logger)
		{
			_logger = logger;
		}

		// Stand-in for a real email provider (no SMTP/SendGrid configured). Logs the
		// message instead of sending it so the link can be copied from the console/log.
		public Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			_logger.LogInformation(
				"Email suppressed (no real sender configured). To: {Email}, Subject: {Subject}, Body: {HtmlMessage}",
				email, subject, htmlMessage);

			return Task.CompletedTask;
		}
	}
}
