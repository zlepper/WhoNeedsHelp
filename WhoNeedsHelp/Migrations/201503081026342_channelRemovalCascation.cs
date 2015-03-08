namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class channelRemovalCascation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("ChatMessages", "ChannelId", "Channels");
            DropForeignKey("Questions", "ChannelId", "Channels");
            AddForeignKey("ChatMessages", "ChannelId", "Channels", "Id", cascadeDelete: true);
            AddForeignKey("Questions", "ChannelId", "Channels", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("Questions", "ChannelId", "Channels");
            DropForeignKey("ChatMessages", "ChannelId", "Channels");
            AddForeignKey("Questions", "ChannelId", "Channels", "Id");
            AddForeignKey("ChatMessages", "ChannelId", "Channels", "Id");
        }
    }
}
