namespace stajprojesi_1.Models
{
    public class Referans
    {
        public int Id { get; set; }
        public string ReferansTuru { get; set; } = string.Empty;
        public string ReferansAdi { get; set; } = string.Empty;
        public bool Silindi { get; set; }
    }
}
