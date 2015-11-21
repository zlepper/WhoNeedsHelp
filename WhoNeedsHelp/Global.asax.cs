using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using Owin;
using System.Web.Optimization;
using System.Web.Security;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Serialization;
using WhoNeedsHelp.DB;
using WhoNeedsHelp.Models;

namespace WhoNeedsHelp
{
    public class Global : System.Web.HttpApplication
    {
        private static void Cleanup()
        {
            using (HelpContext db = new HelpContext())
            {
                foreach (User user in db.Users.Where(u => u.SerializedRoles == "" || u.SerializedRoles == null))
                {
                    var roles = user.Roles.ToList();
                    roles.Add(Role.UserRole);
                    user.Roles = roles.ToArray();
                }
                var usersToRemove = new List<User>();
                foreach (User user in db.Users.Where(u => u.EmailAddress == "" || u.EmailAddress == null).ToList())
                {
                    if (string.IsNullOrWhiteSpace(user.VirtualId))
                    {
                        bool shouldDelete = user.LoginTokens.Any(loginToken => loginToken.CreatedAt.AddDays(31) < DateTime.Now);
                        if (shouldDelete)
                        {
                            usersToRemove.Add(user);
                        }
                    }
                }
                db.Users.RemoveRange(usersToRemove);

                db.Channels.RemoveRange(db.Channels.Where(c => !c.Users.Any()));
            }

        }

        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // Disable recursive references in json
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            // Disable xml generation
            var formatters = GlobalConfiguration.Configuration.Formatters;
            formatters.Remove(formatters.XmlFormatter);

            Cleanup();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {

                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                var serializeModel = JsonConvert.DeserializeObject<PrincipalSerializeModel>(authTicket.UserData);
                using (HelpContext db = new HelpContext())
                {
                    HttpContext.Current.User = db.Users.Find(serializeModel.UserId);
                }

            }

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}