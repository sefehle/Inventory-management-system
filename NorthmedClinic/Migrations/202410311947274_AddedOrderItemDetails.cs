namespace NorthmedClinic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOrderItemDetails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrderItemDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderItemId = c.Int(nullable: false),
                        SupplierId = c.String(nullable: false, maxLength: 128),
                        Quantity = c.Int(nullable: false),
                        Status = c.String(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OrderItems", t => t.OrderItemId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.SupplierId, cascadeDelete: true)
                .Index(t => t.OrderItemId)
                .Index(t => t.SupplierId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderItemDetails", "SupplierId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OrderItemDetails", "OrderItemId", "dbo.OrderItems");
            DropIndex("dbo.OrderItemDetails", new[] { "SupplierId" });
            DropIndex("dbo.OrderItemDetails", new[] { "OrderItemId" });
            DropTable("dbo.OrderItemDetails");
        }
    }
}
