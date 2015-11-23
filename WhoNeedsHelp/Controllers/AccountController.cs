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
using WhoNeedsHelp.Server.Mail;

namespace WhoNeedsHelp.Controllers
{
    public class AccountController : BaseController
    {
        readonly IHelpContext context = new HelpContext();
        // GET: Account
        public ActionResult Index()
        {
            ViewBag.message = TempData["message"];
            var model = new AuthViewModel
            {
                LoginViewModel = new LoginViewModel(),
                SignupViewModel = new SignupViewModel()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
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

        [HttpPost]
        [ValidateAntiForgeryToken()]
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

        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult ResetPassword1(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User u = context.Users.FirstOrDefault(us => us.EmailAddress.Equals(model.Email));
                if (u == null)
                {
                    ModelState.AddModelError("email", "Email addresse blev ikke fundet.");
                }
                else
                {
                    UserMail um = new UserMail();
                    um.SendPasswordRecovery(model.Email);
                    return View("PasswordResetRequested");
                }
            }
            return View("ResetPassword", model);
        }
        
        public ActionResult ResetPassword2(string key, string email)
        {
            // Validate that the user exists
            User u = context.Users.FirstOrDefault(us => us.EmailAddress.Equals(email));
            if (u == null) return RedirectToAction("Index", "Account");

            // Make sure the link is not expired
            if (u.ResetExpiresAt < DateTime.Now)
            {
                ModelState.AddModelError("expired", "Denne nøgle er udløbet.");
                return RedirectToAction("ResetPassword1", "Account");
            }

            if (!PasswordHash.ValidatePassword(key, u.ResetKey))
            {
                ModelState.AddModelError("expired", "Ugyldig nøgle er udløbet.");
                return RedirectToAction("ResetPassword1", "Account");
            }
            ResetPasswordViewModel2 vm = new ResetPasswordViewModel2()
            {
                ResetKey = key,
                Email = email
            };

            return View("ResetPasswordStep1", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult ResetPassword(ResetPasswordViewModel2 model)
        {
            if (ModelState.IsValid)
            {
                // Validate that the user exists
                User u = context.Users.FirstOrDefault(us => us.EmailAddress.Equals(model.Email));
                if (u == null) return RedirectToAction("Index", "Account");

                // Make sure the link is not expired
                if (u.ResetExpiresAt < DateTime.Now)
                {
                    ModelState.AddModelError("expired", "Denne nøgle er udløbet.");
                    return RedirectToAction("ResetPassword1", "Account");
                }

                if (!PasswordHash.ValidatePassword(model.ResetKey, u.ResetKey))
                {
                    ModelState.AddModelError("illegalKey", "Ugyldig nøgle.");
                    return RedirectToAction("ResetPassword1", "Account");
                }
                u.ResetExpiresAt = DateTime.Now;
                u.ResetKey = null;
                u.Pw = PasswordHash.CreateHash(model.Password);
                TempData["message"] = "Log ind med dit nye kodeord.";
                return RedirectToAction("Index", "Account");

            }
            return View("ResetPasswordStep1", model);
        }
    }
}