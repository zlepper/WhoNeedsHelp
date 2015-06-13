namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LoginToken : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Users", "ChannelId", "Channels");
            DropIndex("Users", new[] { "ChannelId" });
            CreateTable(
                "LoginTokens",
                c => new
                    {
                        Key = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Key)                
                .ForeignKey("Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            DropColumn("Users", "ChannelId");
        }
        
        public override void Down()
        {
            AddColumn("Users", "ChannelId", c => c.Int());
            DropForeignKey("LoginTokens", "UserId", "Users");
            DropIndex("LoginTokens", new[] { "UserId" });
            DropTable("LoginTokens");
            CreateIndex("Users", "ChannelId");
            AddForeignKey("Users", "ChannelId", "Channels", "Id");
        }
    }
}
