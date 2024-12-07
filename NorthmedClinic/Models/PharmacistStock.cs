using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NorthmedClinic.Models
{
    public class PharmacistStock
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; }

        public string Dosage { get; set; }

        [Required]
        [Display(Name = "Quantity Available")]
        public int QuantityAvailable { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Expiration Date")]
        public DateTime ExpirationDate { get; set; }

        [Required]
        
        public DateTime LastUpdatedDate { get; set; }

        
        public string BatchCode { get; set; } // Unique identifier for each batch of the same item
    }

    public class TrackExpirationViewModel
    {
        public string Name { get; set; }
        public string BatchCode { get; set; }
        public DateTime ExpirationDate { get; set; }
    }

}