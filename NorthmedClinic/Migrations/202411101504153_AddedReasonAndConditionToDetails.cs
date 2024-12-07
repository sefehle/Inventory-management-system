namespace NorthmedClinic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedReasonAndConditionToDetails : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderItemDetails", "Condition", c => c.String());
            AddColumn("dbo.OrderItemDetails", "ReturnReason", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderItemDetails", "ReturnReason");
            DropColumn("dbo.OrderItemDetails", "Condition");
        }
    }
}
