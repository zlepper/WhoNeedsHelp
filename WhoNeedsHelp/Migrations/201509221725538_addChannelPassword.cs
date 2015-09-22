namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addChannelPassword : DbMigration
    {
        public override void Up()
        {
            AddColumn("Channels", "PasswordHash", c => c.String(unicode: false));
            AddColumn("Channels", "AdminHash", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("Channels", "AdminHash");
            DropColumn("Channels", "PasswordHash");
        }
    }
}
