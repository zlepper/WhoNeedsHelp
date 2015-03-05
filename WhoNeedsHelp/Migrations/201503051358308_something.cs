namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class something : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Questions", "ChannelId", "Channels");
            DropForeignKey("Questions", "UserId", "Users");
            AddForeignKey("Questions", "ChannelId", "Channels", "Id");
            AddForeignKey("Questions", "UserId", "Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("Questions", "UserId", "Users");
            DropForeignKey("Questions", "ChannelId", "Channels");
            AddForeignKey("Questions", "UserId", "Users", "Id", cascadeDelete: true);
            AddForeignKey("Questions", "ChannelId", "Channels", "Id", cascadeDelete: true);
        }
    }
}
