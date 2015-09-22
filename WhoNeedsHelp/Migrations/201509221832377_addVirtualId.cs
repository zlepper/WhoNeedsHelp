namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addVirtualId : DbMigration
    {
        public override void Up()
        {
            AddColumn("Channels", "VirtualId", c => c.String(unicode: false));
            AddColumn("Users", "VirtualId", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("Users", "VirtualId");
            DropColumn("Channels", "VirtualId");
        }
    }
}
