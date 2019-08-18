using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EventManager.Startup))]
namespace EventManager
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
