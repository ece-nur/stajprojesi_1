using System.Collections.Generic;

namespace stajprojesi_1.Models
{
    public class BasvuruModel
    {
        public int Id { get; set; }
        public string Proje_adi { get; set; }
        public string Basvuran_birim { get; set; }
        public string Basvuru_yapilan_proje { get; set; }
        public string Basvuru_yapilan_tur { get; set; }

        public string Katılımcı_turu{ get; set; }
        public string Basvuru_donemi{ get; set; }
        public DateTime Basvuru_tarihi { get; set; }
        public string Basvuru_durumu { get; set; }

        public DateTime Durum_tarihi { get; set; }

        public int Hibe_tutari { get; set; }
        


    }
}
