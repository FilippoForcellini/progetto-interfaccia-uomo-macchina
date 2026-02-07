using System.ComponentModel.DataAnnotations;

namespace PianificazioneTurni.Services.Pianificazione
{
    public class Assegnazione
    {
        [Key]
        public int Id { get; set; }

        public int NaveId { get; set; }
        public Nave Nave { get; set; }

        public int DipendenteId { get; set; }
        public Dipendente Dipendente { get; set; }

        public int Fascia { get; set; } // 0=mattina, 1=pomeriggio, 2=sera
    }
}
