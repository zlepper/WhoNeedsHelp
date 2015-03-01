namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Channels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Users = c.String(unicode: false),
                        UsersRequestingHelp = c.String(unicode: false),
                        Administrators = c.String(unicode: false),
                        ChannelName = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ChatMessages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Text = c.String(unicode: false),
                        Author = c.Guid(nullable: false),
                        Channel = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.QuestionComments",
                c => new
                    {
                        id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Channel = c.Guid(nullable: false),
                        User = c.Guid(nullable: false),
                        Text = c.String(unicode: false),
                        Comments = c.String(unicode: false),
                        AskedTime = c.DateTime(nullable: false, precision: 0),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UserName = c.String(unicode: false),
                        Name = c.String(unicode: false),
                        ChannelId = c.Guid(nullable: false),
                        Questions = c.String(unicode: false),
                        Ip = c.String(unicode: false),
                        ConnectionId = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");
            DropTable("dbo.Questions");
            DropTable("dbo.QuestionComments");
            DropTable("dbo.ChatMessages");
            DropTable("dbo.Channels");
        }
    }
}
