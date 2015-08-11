namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTimeLeft : DbMigration
    {
        public override void Up()
        {
            AddColumn("Channels", "TimeLeft", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Channels", "TimeLeft");
        }
    }
}
