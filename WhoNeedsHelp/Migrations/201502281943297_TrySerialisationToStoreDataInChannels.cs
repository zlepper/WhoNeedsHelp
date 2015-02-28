namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrySerialisationToStoreDataInChannels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Channels", "Users", c => c.String());
            AddColumn("dbo.Channels", "UsersRequestingHelp", c => c.String());
            AddColumn("dbo.Channels", "Administrators", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Channels", "Administrators");
            DropColumn("dbo.Channels", "UsersRequestingHelp");
            DropColumn("dbo.Channels", "Users");
        }
    }
}
