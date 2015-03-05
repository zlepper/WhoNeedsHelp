namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addChatMessageRelationships : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("ChatMessages", "ChannelId", "Channels");
            DropForeignKey("ChatMessages", "UserId", "Users");
            AddForeignKey("ChatMessages", "ChannelId", "Channels", "Id");
            AddForeignKey("ChatMessages", "UserId", "Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("ChatMessages", "UserId", "Users");
            DropForeignKey("ChatMessages", "ChannelId", "Channels");
            AddForeignKey("ChatMessages", "UserId", "Users", "Id", cascadeDelete: true);
            AddForeignKey("ChatMessages", "ChannelId", "Channels", "Id", cascadeDelete: true);
        }
    }
}
