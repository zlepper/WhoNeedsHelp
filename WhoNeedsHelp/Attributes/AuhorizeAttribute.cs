using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WhoNeedsHelp.Models;

namespace WhoNeedsHelp.Attributes
{
    public class AuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        public string UsersConfigKey { get; set; }
        public string RolesConfigKey { get; set; }

        protected virtual User CurrentUser => HttpContext.Current.User as User;

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                if (!string.IsNullOrWhiteSpace(Roles) && !CurrentUser.IsInRole(Roles))
                {
                    filterContext.Result =
                        new RedirectToRouteResult(
                            new RouteValueDictionary(new {controller = "Error", action = "AccessDenied"}));
                }
                else
                {
                    return;
                }
            }
            else
            {
                filterContext.Result =
                    new RedirectToRouteResult(new RouteValueDictionary(new {controller = "Account", action = "Index"}));
            }
            base.OnAuthorization(filterContext);
        }
    }
}
