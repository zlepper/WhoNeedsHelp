namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeMessageIdToId : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.ChatMessages");
            AddColumn("dbo.ChatMessages", "Id", c => c.Guid(nullable: false));
            AddPrimaryKey("dbo.ChatMessages", "Id");
            DropColumn("dbo.ChatMessages", "MessageId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ChatMessages", "MessageId", c => c.Guid(nullable: false));
            DropPrimaryKey("dbo.ChatMessages");
            DropColumn("dbo.ChatMessages", "Id");
            AddPrimaryKey("dbo.ChatMessages", "MessageId");
        }
    }
}
