using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(AgentScrum.Web.Areas.Identity.IdentityHostingStartup))]
namespace AgentScrum.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
} 