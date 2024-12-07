namespace NorthmedClinic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSupplierAndRequestedItems : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RequestedOrderItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItemName = c.String(nullable: false),
                        QuantityRequested = c.Int(nullable: false),
                        OrderId = c.Int(nullable: false),
                        NotifyWhenAvailable = c.Boolean(nullable: false),
                        Notified = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
            
            AddColumn("dbo.OrderItems", "SupplierId", c => c.String(maxLength: 128));
            CreateIndex("dbo.OrderItems", "SupplierId");
            AddForeignKey("dbo.OrderItems", "SupplierId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderItems", "SupplierId", "dbo.AspNetUsers");
            DropForeignKey("dbo.RequestedOrderItems", "OrderId", "dbo.Orders");
            DropIndex("dbo.RequestedOrderItems", new[] { "OrderId" });
            DropIndex("dbo.OrderItems", new[] { "SupplierId" });
            DropColumn("dbo.OrderItems", "SupplierId");
            DropTable("dbo.RequestedOrderItems");
        }
    }
}
