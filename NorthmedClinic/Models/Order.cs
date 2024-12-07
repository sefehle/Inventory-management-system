using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NorthmedClinic.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Pending";
        public bool IsVerified { get; set; } = false; // New property

        // Navigation property for OrderItems
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<RequestedOrderItem> RequestedOrderItems { get; set; } // Out-of-stock items
    }


    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public string ItemName { get; set; }

        [Required]
        public int QuantityOrdered { get; set; }

        public int OrderId { get; set; }

        // Navigation property back to Order
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
        // Item Status
        public string Status { get; set; } = "Pending"; // Default to Pending

        public string SupplierId { get; set; } // Define SupplierId as a foreign key property explicitly


        // Navigation property to Supplier
        [ForeignKey("SupplierId")]
        public virtual ApplicationUser Supplier { get; set; }

        // New collection to store multiple supplier-specific details
        public virtual ICollection<OrderItemDetails> OrderItemDetails { get; set; } = new List<OrderItemDetails>();
    }

    public class OrderItemDetails
    {
        [Key]
        public int Id { get; set; }

        // Foreign key reference to OrderItem
        [Required]
        public int OrderItemId { get; set; }

        [ForeignKey("OrderItemId")]
        public virtual OrderItem OrderItem { get; set; }

        // Supplier selected for this item
        [Required]
        public string SupplierId { get; set; } // Reference to Supplier

        [ForeignKey("SupplierId")]
        public virtual ApplicationUser Supplier { get; set; }

        // Quantity required from this specific supplier
        [Required]
        public int Quantity { get; set; }

        // Current status of this order item detail (e.g., Pending, No Supplier)
        [Required]
        public string Status { get; set; }

        // Price agreed upon for the items from this supplier
        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        // Condition of the received item (e.g., Good, Bad)
        public string Condition { get; set; }

        // Reason for returning the item if condition is "Bad"
        public string ReturnReason { get; set; }
    }

    public class SupplierRecommendation
    {
        public int OrderItemId { get; set; } // Add this property
        public string ItemName { get; set; }
        public int QuantityNeeded { get; set; } // Add this property
                                                // Add this new property for alternative suppliers
        public List<SupplierItemAvailability> AlternativeSuppliers { get; set; }
        public List<SupplierItemAvailability> Suppliers { get; set; }
        public bool FullyFulfilled { get; set; } // Indicates if the item is fully fulfilled by suppliers
    }

    //for unavailable orderitems
    public class RequestedOrderItem
    {
        public int Id { get; set; }

        [Required]
        public string ItemName { get; set; }

        [Required]
        public int QuantityRequested { get; set; }

        public int OrderId { get; set; }

        // Tracks if the pharmacist wants to be notified when this item becomes available
        public bool NotifyWhenAvailable { get; set; }

        // Tracks if the pharmacist has been notified after the item became available
        public bool Notified { get; set; }

        // Navigation property back to Order
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }

    

    public class SupplierSelectionViewModel
    {
        public int OrderItemId { get; set; }
        public string SupplierId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ItemName { get; set; } // Add ItemName to track the item being ordered
    }

    public class ManageOrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string ItemName { get; set; }
        public int QuantityOrdered { get; set; }
        public string Status { get; set; }
        public string SupplierName { get; set; }
        public bool IsVerified { get; set; }
    }

    public class VerifyOrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public List<VerifyOrderItemViewModel> OrderItems { get; set; }
        public ICollection<OrderItemDetails> OrderItemDetails { get; set; }

    }

    public class VerifyOrderItemViewModel
    {
        public int OrderItemId { get; set; }
        public string ItemName { get; set; }
        
        public int QuantityOrdered { get; set; }
        public List<VerifyOrderItemDetailViewModel> OrderItemDetails { get; set; }


    }

    public class VerifyOrderItemDetailViewModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public string SupplierName { get; set; }
        public string Status { get; set; }
        public string Condition { get; set; } = "Good";  // The condition of the item ("Good" or "Bad")
        public string ReturnReason { get; set; }  // Reason for return if the item is "Bad"
    }


}