using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PianificazioneTurni.Services.Pianificazione
{
    public class Nave
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        public int Tipo { get; set; } // TipoNave enum as int

        public int? Pontile { get; set; }

        public DateTime? DataArrivo { get; set; }
        public int OrarioArrivo { get; set; } // 0, 8, 16

        public DateTime? DataPartenza { get; set; }
        public int OrarioPartenza { get; set; } // 8, 16, 24

        public bool RichiedeGruisti { get; set; }
        public bool RichiedeMulettisti { get; set; }
        public bool RichiedeAddettiTerminal { get; set; }
        public bool RichiedeOrmeggiatori { get; set; }
        public bool RichiedeAddettiSicurezza { get; set; }

        // Relazione con assegnazioni
        public ICollection<Assegnazione> Assegnazioni { get; set; } = new List<Assegnazione>();
    }
}
