namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class multipleConnections : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Connections",
                c => new
                    {
                        ConnectionId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ConnectionId)                
                .ForeignKey("Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            DropColumn("Users", "ConnectionId");
        }
        
        public override void Down()
        {
            AddColumn("Users", "ConnectionId", c => c.String(unicode: false));
            DropForeignKey("Connections", "UserId", "Users");
            DropIndex("Connections", new[] { "UserId" });
            DropTable("Connections");
        }
    }
}
