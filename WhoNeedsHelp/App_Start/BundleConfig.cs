using System.Runtime.InteropServices.ComTypes;
using System.Web.Optimization;

namespace WhoNeedsHelp
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/deps").Include(
                        "~/Scripts/analytics.js",
                        "~/Scripts/jquery-2.1.4.min.js",
                        "~/Scripts/jquery-ui-1.11.4.min.js",
                        "~/Scripts/jquery.signalR-2.2.0.min.js",
                        "~/Scripts/pnotify.custom.min.js",
                        "~/Scripts/angular.js",
                        "~/Scripts/angular-animate.js",
                        "~/Scripts/angular-cookies.js",
                        "~/Scripts/modernizr-2.8.3.js",
                        "~/js/helpers.js",
                        "~/js/notifications.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/classes").Include(
                        "~/js/modules/zl-features.js",
                        "~/js/modules/zl-signalr.js",
                        "~/js/modules/zl-help.js",
                        "~/js/classes/chat-message.js",
                        "~/js/cleanup-timer.js",
                        "~/js/classes/login-token.js",
                        "~/js/classes/login-option.js",
                        "~/js/classes/me.js",
                        "~/js/classes/question.js",
                        "~/js/classes/user.js",
                        "~/js/classes/student-timer.js",
                        "~/js/classes/channel.js",
                        "~/js/classes/application.js",
                        "~/js/countdown.js",
                        "~/js/directives/ask-question.js",
                        "~/js/directives/editQuestionModal.js",
                        "~/js/directives/loading.js",
                        "~/js/directives/login.js",
                        "~/js/directives/navbar.js",
                        "~/js/directives/questions.js",
                        "~/js/directives/timer.js",
                        "~/js/directives/usermanage.js",
                        "~/js/directives/chat.js"
                        ));

            bundles.Add(new ScriptBundle("~/helper").Include("~/js/controllers/helpctrl.js"));
            bundles.Add(new ScriptBundle("~/apihelper").Include("~/js/controllers/apictrl.js"));

            bundles.Add(new StyleBundle("~/bundles/styling").Include(
                        "~/Content/animate.min.css",
                        "~/css/custom.css",
                        "~/css/loading.css"));

            bundles.Add(new StyleBundle("~/bundles/home").Include());
        }
    }
}
