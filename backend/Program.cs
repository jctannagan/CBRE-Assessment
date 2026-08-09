using CBRE.TaskListDemo.Application.Interfaces;
using CBRE.TaskListDemo.Core.Entities;
using CBRE.TaskListDemo.Infrastructure.Data;
using CBRE.TaskListDemo.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Add Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
	?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseNpgsql(connectionString)
);

// Add Identity services
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
	// Policy settings
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequiredLength = 8;
	options.User.RequireUniqueEmail = true;
})
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();

// Setup JWT Bearer Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		// .NET 8+ defaults this to false, which stops "sub" from mapping to
		// ClaimTypes.NameIdentifier. Controllers rely on that mapping.
		options.MapInboundClaims = true;
		options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = jwtSection["Issuer"],
			ValidAudience = jwtSection["Audience"],
			IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSection["Key"]))
		};
	});

builder.Services.AddAuthorization();

// Setup CORS for the Angular dev server
const string AngularCorsPolicy = "AngularClient";
builder.Services.AddCors(options =>
{
	options.AddPolicy(AngularCorsPolicy, policy =>
	{
		policy.WithOrigins(builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:4200")
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials();
	});
});

builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
	});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors(AngularCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/claims", (HttpContext context) =>
{
	var clms = context.User.Claims.Select(c => new
	{
		c.Type,
		c.Value
	});

	return Results.Ok(clms);
});

// ---- Seed roles on startup
using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	foreach (var role in new[] { "Admin", "User" })
	{
		if (!await roleManager.RoleExistsAsync(role))
			await roleManager.CreateAsync(new IdentityRole(role));
	}
}

app.Run();