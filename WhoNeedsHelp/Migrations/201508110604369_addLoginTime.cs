namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addLoginTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("Users", "LastLogin", c => c.DateTime(nullable: false, precision: 0));
        }
        
        public override void Down()
        {
            DropColumn("Users", "LastLogin");
        }
    }
}
