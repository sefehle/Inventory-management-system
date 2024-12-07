namespace NorthmedClinic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSupplierNameToApplicationUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "SupplierName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "SupplierName");
        }
    }
}
