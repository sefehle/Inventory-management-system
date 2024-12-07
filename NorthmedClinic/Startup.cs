using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NorthmedClinic.Startup))]
namespace NorthmedClinic
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
