using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WhoNeedsHelp.Models;

namespace WhoNeedsHelp.Controllers
{
    public class BaseController : Controller
    {
        protected virtual User user => HttpContext.User as User;


    }
}
