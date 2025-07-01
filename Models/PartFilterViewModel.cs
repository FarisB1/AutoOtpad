using Microsoft.AspNetCore.Mvc;

namespace AutoOtpad.Models
{
    public class PartFilterViewModel
    {
        public List<Part> Parts { get; set; } = new();
        public string? Search { get; set; }
        public string? VehicleMake { get; set; }
        public string? Condition { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public List<string> AvailableMakes { get; set; } = new();
        public List<string> AvailableConditions { get; set; } = new();
    }

}
