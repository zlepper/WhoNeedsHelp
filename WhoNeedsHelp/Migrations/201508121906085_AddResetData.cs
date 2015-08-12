namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddResetData : DbMigration
    {
        public override void Up()
        {
            AddColumn("Users", "CreatedAt", c => c.DateTime(nullable: false, precision: 0));
            AddColumn("Users", "ResetExpiresAt", c => c.DateTime(nullable: false, precision: 0));
            AddColumn("Users", "ResetKey", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("Users", "ResetKey");
            DropColumn("Users", "ResetExpiresAt");
            DropColumn("Users", "CreatedAt");
        }
    }
}
