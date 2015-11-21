using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WhoNeedsHelp.Models;

namespace WhoNeedsHelp.Views
{
    public abstract class BaseViewPage : WebViewPage
    {
        public new virtual User User => base.User as User;
    }
    public abstract class BaseViewPage<TModel> : WebViewPage<TModel>
    {
        public new virtual User User => base.User as User;
    }
}

