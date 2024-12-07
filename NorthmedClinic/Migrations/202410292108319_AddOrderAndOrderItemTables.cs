namespace NorthmedClinic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrderAndOrderItemTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrderItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItemName = c.String(nullable: false),
                        QuantityOrdered = c.Int(nullable: false),
                        OrderId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
            
            DropColumn("dbo.Orders", "ItemName");
            DropColumn("dbo.Orders", "QuantityOrdered");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "QuantityOrdered", c => c.Int(nullable: false));
            AddColumn("dbo.Orders", "ItemName", c => c.String(nullable: false));
            DropForeignKey("dbo.OrderItems", "OrderId", "dbo.Orders");
            DropIndex("dbo.OrderItems", new[] { "OrderId" });
            DropTable("dbo.OrderItems");
        }
    }
}
