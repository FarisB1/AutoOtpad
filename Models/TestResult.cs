namespace AutoOtpad.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public Part Part { get; set; }
        public DateTime TestDate { get; set; }
        public string Result { get; set; } // Pass, Fail
        public string Notes { get; set; }
    }

}
