using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Promo.EverythingIsNew.WebApp.Startup))]
namespace Promo.EverythingIsNew.WebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
