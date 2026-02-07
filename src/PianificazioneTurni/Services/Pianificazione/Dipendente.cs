using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PianificazioneTurni.Services.Pianificazione
{
    public class Dipendente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        public string Ruolo { get; set; }

        public bool Patente { get; set; }

        public DateTime? Scadenza { get; set; }

        // Relazione con assegnazioni
        public ICollection<Assegnazione> Assegnazioni { get; set; } = new List<Assegnazione>();
    }
}
