namespace NorthmedClinic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedBatchCodeToStock : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PharmacistStocks", "BatchCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PharmacistStocks", "BatchCode");
        }
    }
}
