using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SurveillanceSystemWebApp.Startup))]
namespace SurveillanceSystemWebApp
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
