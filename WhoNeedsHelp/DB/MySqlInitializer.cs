using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using Configuration = WhoNeedsHelp.Migrations.Configuration;

namespace WhoNeedsHelp.DB
{
    public class MySqlInitializer : IDatabaseInitializer<HelpContext>
    {
        public void InitializeDatabase(HelpContext context)
        {
            if (!context.Database.Exists())
            {
                // if database did not exist before - create it
                context.Database.Create();
            }
            else
            {
                // query to check if MigrationHistory table is present in the database 
                var migrationHistoryTableExists = ((IObjectContextAdapter)context).ObjectContext.ExecuteStoreQuery<int>(
                string.Format(
                  "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{0}' AND table_name = '__MigrationHistory'",
                  ConfigurationManager.AppSettings["dbname"]));

                // if MigrationHistory table is not there (which is the case first time we run) - create it
                if (migrationHistoryTableExists.FirstOrDefault() == 0)
                {
                    context.Database.Delete();
                    context.Database.Create();
                }

                var config = new Configuration();

                var migratror = new DbMigrator(config);
                var pending = migratror.GetPendingMigrations();
                Console.WriteLine(pending.Count());
                migratror.Update();

            }
        }
    }
}