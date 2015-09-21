using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using MySql.Data.Entity;
using System.Web.Http;
using WhoNeedsHelp.Server.Mail;

[assembly: OwinStartup(typeof(WhoNeedsHelp.Startup))]

namespace WhoNeedsHelp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseFileServer(new FileServerOptions()
            {
                RequestPath = PathString.Empty,
                FileSystem = new PhysicalFileSystem(@".\public")
            });

            app.UseStaticFiles("/css");
            app.UseStaticFiles("/Content");
            app.UseStaticFiles("/fonts");
            app.UseStaticFiles("/parts");
            app.UseStaticFiles("/public");
            app.UseStaticFiles("/Scripts");
            app.UseStaticFiles("/templates");


            Database.SetInitializer( new CreateDatabaseIfNotExists<HelpContext>());
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<HelpContext, Migrations.Configuration>());
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            
            app.MapSignalR();
            var m = new UserMail();
        }

        public static Task ApiInvoke(IOwinContext context)
        {
            context.Response.ContentType = "text/html";
            return context.Response.SendFileAsync(@"~\public\api.html");
        }
    }


}
