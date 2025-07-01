using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoOtpad.Models
{
    public class Promotion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv promocije je obavezan.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Morate izabrati dio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Morate izabrati dio.")]
        public int PartId { get; set; }

        [ForeignKey("PartId")]
        public Part? Part { get; set; }

        [Range(1, 100, ErrorMessage = "Popust mora biti između 1% i 100%.")]
        public decimal DiscountPercentage { get; set; }

        [Required(ErrorMessage = "Unesite datum početka.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Unesite datum kraja.")]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
    }
}