using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pianificazioneturni.Web.Areas.Example.Users
{
    public class GestioneDipendentiViewModel
    {
        public GestioneDipendentiViewModel()
        {
            Dipendenti = new List<DipendenteDetailViewModel>();
            FiltroPatentiScadute = false;
        }

        public List<DipendenteDetailViewModel> Dipendenti { get; set; }
        public bool FiltroPatentiScadute { get; set; }

        public List<DipendenteDetailViewModel> GetDipendentiFiltrati()
        {
            if (!FiltroPatentiScadute)
                return Dipendenti;

            return Dipendenti.FindAll(d => d.RichiedePatente && d.Patente && d.IsPatenteScaduta);
        }
    }

    public class DipendenteDetailViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nome")]
        public string Nome { get; set; }

        [Display(Name = "Ruolo")]
        public string Ruolo { get; set; }

        [Display(Name = "Patente")]
        public bool Patente { get; set; }

        [Display(Name = "Scadenza")]
        [DataType(DataType.Date)]
        public DateTime? Scadenza { get; set; }

        public bool RichiedePatente => Ruolo == "Gruista" || Ruolo == "Mulettista";

        public bool IsPatenteScaduta => RichiedePatente && Patente && Scadenza.HasValue && Scadenza.Value.Date < DateTime.Today;

        public string ScadenzaFormattata => Scadenza.HasValue ? Scadenza.Value.ToString("dd/MM/yyyy") : "-";
    }

    public static class RuoliDipendente
    {
        public static List<string> GetRuoli()
        {
            return new List<string>
            {
                "Gruista",
                "Mulettista",
                "Addetto terminal",
                "Ormeggiatore",
                "Addetto alla Sicurezza"
            };
        }
    }
}
