using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Pianificazioneturni.Web.Areas.Example.Users
{
    public class GestioneNaviViewModel
    {
        public GestioneNaviViewModel()
        {
            Navi = new List<NaveDetailViewModel>();
        }

        public List<NaveDetailViewModel> Navi { get; set; }
    }

    public class NaveDetailViewModel
    {
        public NaveDetailViewModel()
        {
            GiorniSosta = new List<DateTime>();
        }

        public int Id { get; set; }

        [Display(Name = "Nome Nave")]
        [Required(ErrorMessage = "Il nome della nave è obbligatorio")]
        public string Nome { get; set; }

        [Display(Name = "Pontile")]
        [Required(ErrorMessage = "Il numero del pontile è obbligatorio")]
        public int? Pontile { get; set; }

        [Display(Name = "Giorni di Sosta")]
        public List<DateTime> GiorniSosta { get; set; }

        [Display(Name = "Richiede Gruisti")]
        public bool RichiedeGruisti { get; set; }

        [Display(Name = "Richiede Mulettisti")]
        public bool RichiedeMulettisti { get; set; }

        // Proprietà per visualizzazione
        public string RuoliRichiesti
        {
            get
            {
                var ruoli = new List<string>();
                if (RichiedeGruisti) ruoli.Add("Gruisti");
                if (RichiedeMulettisti) ruoli.Add("Mulettisti");
                return ruoli.Count > 0 ? string.Join(", ", ruoli) : "-";
            }
        }

        public string GiorniSostaFormattati
        {
            get
            {
                if (GiorniSosta == null || GiorniSosta.Count == 0)
                    return "-";

                var giorniOrdinati = GiorniSosta.OrderBy(d => d).ToList();
                if (giorniOrdinati.Count <= 3)
                    return string.Join(", ", giorniOrdinati.Select(d => d.ToString("dd/MM")));

                return $"{giorniOrdinati.First():dd/MM} - {giorniOrdinati.Last():dd/MM}";
            }
        }

        // Colore assegnato alla nave per il calendario
        public string Colore { get; set; }
    }

    public static class ColoriNavi
    {
        private static readonly List<string> _colori = new List<string>
        {
            "#FF6B6B", // Rosso
            "#4ECDC4", // Turchese
            "#45B7D1", // Azzurro
            "#96CEB4", // Verde
            "#FFEAA7", // Giallo
            "#DDA0DD", // Viola chiaro
            "#98D8C8", // Verde acqua
            "#F7DC6F", // Oro
            "#BB8FCE", // Lavanda
            "#85C1E9"  // Celeste
        };

        public static string GetColore(int index)
        {
            return _colori[index % _colori.Count];
        }
    }
}
