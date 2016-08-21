namespace IEP_Projekat.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TokenNum : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "TokenNum", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "TokenNum");
        }
    }
}
