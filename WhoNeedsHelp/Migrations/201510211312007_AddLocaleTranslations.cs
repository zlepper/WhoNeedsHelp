namespace WhoNeedsHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLocaleTranslations : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Locales",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Language = c.String(unicode: false),
                        LanguageId = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id)                ;
            
            CreateTable(
                "Translations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.String(unicode: false),
                        Value = c.String(unicode: false),
                        LocaleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)                
                .ForeignKey("Locales", t => t.LocaleId)
                .Index(t => t.LocaleId);
            
            AddColumn("Users", "PreferedLocaleId", c => c.Int());
            CreateIndex("Users", "PreferedLocaleId");
            AddForeignKey("Users", "PreferedLocaleId", "Locales", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("Users", "PreferedLocaleId", "Locales");
            DropForeignKey("Translations", "LocaleId", "Locales");
            DropIndex("Translations", new[] { "LocaleId" });
            DropIndex("Users", new[] { "PreferedLocaleId" });
            DropColumn("Users", "PreferedLocaleId");
            DropTable("Translations");
            DropTable("Locales");
        }
    }
}
