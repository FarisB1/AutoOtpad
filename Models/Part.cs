using System.ComponentModel.DataAnnotations.Schema;

namespace AutoOtpad.Models
{
    public class Part
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string VehicleMake { get; set; }
        public string PartType { get; set; }
        public string Condition { get; set; } // New, Used, etc.
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
        public bool IsTested { get; set; }
        public string Status { get; set; } // Available, Reserved, Sold
        public string? ImagePath { get; set; } // stores image filename or URL
        public string? TestResult { get; set; }
        public int ReorderLevel { get; set; } = 2; // minimalna količina prije dopune
        public bool NeedsRestock { get; set; } = false;
        
        [NotMapped]
        public decimal? DiscountedPrice { get; set; }


    }
}
