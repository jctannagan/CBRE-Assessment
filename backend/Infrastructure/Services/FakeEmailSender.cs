using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace CBRE.TaskListDemo.Infrastructure.Services
{
	public class FakeEmailSender : IEmailSender
	{
		public Task SendEmailAsync(string email, string subject, string htmlMessage) => Task.CompletedTask;
	}
}
