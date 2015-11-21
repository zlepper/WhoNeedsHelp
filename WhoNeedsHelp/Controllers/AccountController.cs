using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using WhoNeedsHelp.DB;
using WhoNeedsHelp.Models;
using WhoNeedsHelp.Server.Chat;

namespace WhoNeedsHelp.Controllers
{
    public class AccountController : BaseController
    {
        readonly IHelpContext context = new HelpContext();
        // GET: Account
        public ActionResult Index()
        {
            var model = new AuthViewModel
            {
                LoginViewModel = new LoginViewModel(),
                SignupViewModel = new SignupViewModel()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model, string returnUrl = "")
        {
            if (ModelState.IsValid)
            {
                var u = context.Users.FirstOrDefault(us => us.EmailAddress.Equals(model.Email));
                if(u != null && PasswordHash.ValidatePassword(model.Password, u.Pw))
                {
                    var roles = u.Roles;
                    var serializeModel = new PrincipalSerializeModel()
                    {
                        UserId = u.Id,
                        Roles = roles
                    };

                    string userData = JsonConvert.SerializeObject(serializeModel);
                    FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, u.EmailAddress, DateTime.Now, DateTime.Now.AddMinutes(15), model.RememberMe, userData);
                    string encTicket = FormsAuthentication.Encrypt(authTicket);
                    HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                    Response.Cookies.Add(faCookie);
                    if (!string.IsNullOrWhiteSpace(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    if (roles.Contains(Role.AdminRole))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Index", "User");
                    }
                }
                ModelState.AddModelError("login", "Forkert email og/eller kodeord.");
            }
            var m = new AuthViewModel
            {
                LoginViewModel = model,
                SignupViewModel = new SignupViewModel()
            };
            return View("Index", m);
        }

        public ActionResult Signup(SignupViewModel model, string returnUrl = "")
        {
            if (ModelState.IsValid)
            {
                if (context.Users.Any(u => u.EmailAddress.Equals(model.Email)))
                {
                    ModelState.AddModelError("email", "Denne email er allerede i brug.");
                }
                else
                {
                    User u = new User
                    {
                        Roles = new[] {Role.UserRole},
                        EmailAddress = model.Email,
                        Pw = PasswordHash.CreateHash(model.Password),
                        Name = model.Name
                    };
                    context.Users.Add(u);
                    context.SaveChanges();

                    var serializeModel = new PrincipalSerializeModel()
                    {
                        UserId = u.Id,
                        Roles = u.Roles
                    };

                    string userData = JsonConvert.SerializeObject(serializeModel);
                    FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, u.EmailAddress, DateTime.Now, DateTime.Now.AddMinutes(15), model.RememberMe, userData);
                    string encTicket = FormsAuthentication.Encrypt(authTicket);
                    HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                    Response.Cookies.Add(faCookie);
                    if (!string.IsNullOrWhiteSpace(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "User");
                }
            }

            AuthViewModel m = new AuthViewModel
            {
                LoginViewModel = new LoginViewModel(),
                SignupViewModel = model
            };
            
            return View("Index", m);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            context.SaveChanges();
            context.Dispose();
            base.Dispose(disposing);
        }
    }
}