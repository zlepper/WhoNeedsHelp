using System.Web.Optimization;

namespace WhoNeedsHelp.App_Start
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
                        "~/signalr/hubs",
                        "~/signalr/js",
                        "~/Scripts/pnotify.custom.min.js",
                        "~/Scripts/angular.js",
                        "~/Scripts/angular-animate.js",
                        "~/Scripts/angular-cookies.js",
                        "~/Scripts/modernizr-2.8.3.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/mainapp").Include(
                        "~/js/zl-features.js",
                        "~/js/ServerActions.js",
                        "~/js/IHelpScope.js",
                        "~/js/SignalR.js",
                        "~/js/ICentralHubProxy.js",
                        "~/js/ICentralClient.js",
                        "~/js/ICentralServer.js",
                        "~/js/LoginOptions.js",
                        "~/js/LoginToken.js",
                        "~/js/Me.js",
                        "~/js/User.js",
                        "~/js/Question.js",
                        "~/js/QuestionState.js",
                        "~/js/Channel.js",
                        "~/js/ChatMessage.js",
                        "~/js/helpers.js",
                        "~/js/directives/timer.js",
                        "~/js/directives/navbar.js",
                        "~/js/directives/loading.js",
                        "~/js/directives/questions.js",
                        "~/js/directives/ask-question.js",
                        "~/js/directives/editQuestionModal.js",
                        "~/js/directives/chat.js",
                        "~/js/directives/usermanage.js",
                        "~/js/directives/login.js",
                        "~/js/countdown.js",
                        "~/js/loading.js"
                        ));

            bundles.Add(new ScriptBundle("~/helper").Include("~/js/helper.js"));
            bundles.Add(new ScriptBundle("~/apihelper").Include("~/js/api.js"));

            bundles.Add(new StyleBundle("~/bundles/styling").Include(
                        "~/Content/animate.min.css",
                        "~/css/custom.css",
                        "~/css/loading.css"));

            bundles.Add(new StyleBundle("~/bundles/home").Include());
        }
    }
}
