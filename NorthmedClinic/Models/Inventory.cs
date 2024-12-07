using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NorthmedClinic.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        
        public string SupplierId { get; set; } // Foreign Key to ApplicationUser (Supplier)

        [Required]
        [Display(Name ="Item Name")]
        public string ItemName { get; set; }

        public string Dosage { get; set; }

        [Required]
        public int Quantity { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Expiration Date")]
        public DateTime ExpirationDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        // Navigation property to the Supplier
        [ForeignKey("SupplierId")]
        public virtual ApplicationUser Supplier { get; set; }
    }

    public class SupplierItemAvailability
    {
        public string SupplierId { get; set; } // Add this property for Supplier reference
        public string SupplierName { get; set; }
        public string ItemName { get; set; }
        public int QuantityAvailable { get; set; }
        public decimal Price { get; set; }
        public int QuantityNeeded { get; set; } // New property for the ordered quantity
    }

}