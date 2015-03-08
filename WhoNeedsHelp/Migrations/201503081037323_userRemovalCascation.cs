namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userRemovalCascation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("ChatMessages", "UserId", "Users");
            DropForeignKey("Questions", "UserId", "Users");
            AddForeignKey("ChatMessages", "UserId", "Users", "Id", cascadeDelete: true);
            AddForeignKey("Questions", "UserId", "Users", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("Questions", "UserId", "Users");
            DropForeignKey("ChatMessages", "UserId", "Users");
            AddForeignKey("Questions", "UserId", "Users", "Id");
            AddForeignKey("ChatMessages", "UserId", "Users", "Id");
        }
    }
}
