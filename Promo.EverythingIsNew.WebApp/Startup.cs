using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartup(typeof(OAuthProvidersWithoutIdentity.Startup))]

namespace OAuthProvidersWithoutIdentity
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.SetDefaultSignInAsAuthenticationType("ExternalCookie");
            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    AuthenticationType = "ExternalCookie",
            //    AuthenticationMode = AuthenticationMode.Passive,
            //    CookieName = ".AspNet.ExternalCookie",
            //    ExpireTimeSpan = TimeSpan.FromMinutes(5),
            //});

            

        }
    }
}   