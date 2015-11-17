using System.Data.Entity;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using WhoNeedsHelp;
using WhoNeedsHelp.DB;
using WhoNeedsHelp.Server.Mail;

[assembly: OwinStartup(typeof(Startup))]

namespace WhoNeedsHelp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseStaticFiles("/css");
            app.UseStaticFiles("/Content");
            app.UseStaticFiles("/parts");
            app.UseStaticFiles("/Scripts");
            app.UseStaticFiles("/templates");


            Database.SetInitializer( new CreateDatabaseIfNotExists<HelpContext>());
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<HelpContext, Migrations.Configuration>());
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            
            app.MapSignalR();
            var m = new UserMail();
        }
    }


}
