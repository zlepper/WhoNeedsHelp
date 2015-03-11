namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addEmailAddressToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("Users", "EmailAddress", c => c.String(unicode: false));
            DropColumn("Users", "UserName");
        }
        
        public override void Down()
        {
            AddColumn("Users", "UserName", c => c.String(unicode: false));
            DropColumn("Users", "EmailAddress");
        }
    }
}
