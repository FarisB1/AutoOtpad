namespace AutoOtpad.Models
{
    public class QualityLog
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public Part Part { get; set; }
        public DateTime DateLogged { get; set; }
        public string Issue { get; set; }
        public string Resolution { get; set; }
    }

}
