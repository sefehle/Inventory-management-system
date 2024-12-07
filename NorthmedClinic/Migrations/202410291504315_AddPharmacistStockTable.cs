namespace NorthmedClinic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPharmacistStockTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PharmacistStocks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItemName = c.String(nullable: false),
                        Dosage = c.String(),
                        QuantityAvailable = c.Int(nullable: false),
                        ExpirationDate = c.DateTime(nullable: false),
                        LastUpdatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PharmacistStocks");
        }
    }
}
