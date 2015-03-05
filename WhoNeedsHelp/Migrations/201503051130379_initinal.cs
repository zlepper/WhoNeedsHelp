namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initinal : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Channels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChannelName = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id)                ;
            
            CreateTable(
                "Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserName = c.String(unicode: false),
                        Name = c.String(unicode: false),
                        ChannelId = c.Int(),
                        Pw = c.String(unicode: false),
                        Ip = c.String(unicode: false),
                        ConnectionId = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id)                
                .ForeignKey("Channels", t => t.ChannelId)
                .Index(t => t.ChannelId);
            
            CreateTable(
                "Questions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChannelId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        Text = c.String(unicode: false),
                        Comments = c.String(unicode: false),
                        AskedTime = c.DateTime(nullable: false, precision: 0),
                    })
                .PrimaryKey(t => t.Id)                
                .ForeignKey("Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("Channels", t => t.ChannelId, cascadeDelete: true)
                .Index(t => t.ChannelId)
                .Index(t => t.UserId);
            
            CreateTable(
                "ChatMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(unicode: false),
                        UserId = c.Int(nullable: false),
                        ChannelId = c.Int(nullable: false),
                        Time = c.DateTime(nullable: false, precision: 0),
                    })
                .PrimaryKey(t => t.Id)                
                .ForeignKey("Channels", t => t.ChannelId, cascadeDelete: true)
                .ForeignKey("Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ChannelId);
            
            CreateTable(
                "AdministratorsInChannels",
                c => new
                    {
                        UserRefId = c.Int(nullable: false),
                        ChannelRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserRefId, t.ChannelRefId })                
                .ForeignKey("Users", t => t.UserRefId, cascadeDelete: true)
                .ForeignKey("Channels", t => t.ChannelRefId, cascadeDelete: true)
                .Index(t => t.UserRefId)
                .Index(t => t.ChannelRefId);
            
            CreateTable(
                "UsersInChannels",
                c => new
                    {
                        UserRefId = c.Int(nullable: false),
                        ChannelRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserRefId, t.ChannelRefId })                
                .ForeignKey("Users", t => t.UserRefId, cascadeDelete: true)
                .ForeignKey("Channels", t => t.ChannelRefId, cascadeDelete: true)
                .Index(t => t.UserRefId)
                .Index(t => t.ChannelRefId);
            
            CreateTable(
                "UsersRequestingHelpInChannels",
                c => new
                    {
                        UserRefId = c.Int(nullable: false),
                        ChannelRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserRefId, t.ChannelRefId })                
                .ForeignKey("Users", t => t.UserRefId, cascadeDelete: true)
                .ForeignKey("Channels", t => t.ChannelRefId, cascadeDelete: true)
                .Index(t => t.UserRefId)
                .Index(t => t.ChannelRefId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("ChatMessages", "UserId", "Users");
            DropForeignKey("ChatMessages", "ChannelId", "Channels");
            DropForeignKey("Questions", "ChannelId", "Channels");
            DropForeignKey("Questions", "UserId", "Users");
            DropForeignKey("UsersRequestingHelpInChannels", "ChannelRefId", "Channels");
            DropForeignKey("UsersRequestingHelpInChannels", "UserRefId", "Users");
            DropForeignKey("UsersInChannels", "ChannelRefId", "Channels");
            DropForeignKey("UsersInChannels", "UserRefId", "Users");
            DropForeignKey("Users", "ChannelId", "Channels");
            DropForeignKey("AdministratorsInChannels", "ChannelRefId", "Channels");
            DropForeignKey("AdministratorsInChannels", "UserRefId", "Users");
            DropIndex("UsersRequestingHelpInChannels", new[] { "ChannelRefId" });
            DropIndex("UsersRequestingHelpInChannels", new[] { "UserRefId" });
            DropIndex("UsersInChannels", new[] { "ChannelRefId" });
            DropIndex("UsersInChannels", new[] { "UserRefId" });
            DropIndex("AdministratorsInChannels", new[] { "ChannelRefId" });
            DropIndex("AdministratorsInChannels", new[] { "UserRefId" });
            DropIndex("ChatMessages", new[] { "ChannelId" });
            DropIndex("ChatMessages", new[] { "UserId" });
            DropIndex("Questions", new[] { "UserId" });
            DropIndex("Questions", new[] { "ChannelId" });
            DropIndex("Users", new[] { "ChannelId" });
            DropTable("UsersRequestingHelpInChannels");
            DropTable("UsersInChannels");
            DropTable("AdministratorsInChannels");
            DropTable("ChatMessages");
            DropTable("Questions");
            DropTable("Users");
            DropTable("Channels");
        }
    }
}
