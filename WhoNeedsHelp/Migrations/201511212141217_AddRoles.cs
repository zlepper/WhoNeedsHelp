namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRoles : DbMigration
    {
        public override void Up()
        {
            AddColumn("Users", "SerializedRoles", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("Users", "SerializedRoles");
        }
    }
}
