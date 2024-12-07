namespace NorthmedClinic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsVerifiedtoOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "IsVerified", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "IsVerified");
        }
    }
}
