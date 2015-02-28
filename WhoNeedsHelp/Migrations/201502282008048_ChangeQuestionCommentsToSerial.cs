namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeQuestionCommentsToSerial : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Questions", "Comments", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Questions", "Comments");
        }
    }
}
