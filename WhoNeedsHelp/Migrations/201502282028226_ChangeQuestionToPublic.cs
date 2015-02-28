namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeQuestionToPublic : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Questions", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Questions");
        }
    }
}
