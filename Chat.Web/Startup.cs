using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Chat.Web.Startup))]
namespace Chat.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
