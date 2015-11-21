using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WhoNeedsHelp.Models;

namespace WhoNeedsHelp.Controllers
{
    [Attributes.Authorize(Roles = Role.UserRole)]
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }
    }
}