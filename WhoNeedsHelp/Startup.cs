using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using MySql.Data.Entity;

[assembly: OwinStartup(typeof(WhoNeedsHelp.Startup))]

namespace WhoNeedsHelp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Database.SetInitializer<HelpContext>( new CreateDatabaseIfNotExists<HelpContext>());
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<HelpContext, Migrations.Configuration>());
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            app.MapSignalR();
        }
    }
}
