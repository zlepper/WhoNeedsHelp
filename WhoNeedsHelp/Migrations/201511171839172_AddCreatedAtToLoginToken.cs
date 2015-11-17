namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCreatedAtToLoginToken : DbMigration
    {
        public override void Up()
        {
            AddColumn("LoginTokens", "CreatedAt", c => c.DateTime(nullable: false, precision: 0));
        }
        
        public override void Down()
        {
            DropColumn("LoginTokens", "CreatedAt");
        }
    }
}
