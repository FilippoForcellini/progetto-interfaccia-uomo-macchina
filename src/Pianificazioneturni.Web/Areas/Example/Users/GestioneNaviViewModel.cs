using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Pianificazioneturni.Web.Areas.Example.Users
{
    // Enum per il tipo di nave
    public enum TipoNave
    {
        Container,
        Portarinfuse,
        NaveTraghetto,
        Petroliera,
        CargoPronta
    }

    public class GestioneNaviViewModel
    {
        public GestioneNaviViewModel()
        {
            NaviOggi = new List<NaveDetailViewModel>();
            NaviDomani = new List<NaveDetailViewModel>();
            TutteLeNavi = new List<NaveDetailViewModel>();
            GiorniFuturiConNavi = new List<GiornoConNaviViewModel>();
        }

        public DateTime DataOggi { get; set; } = DateTime.Today;
        public DateTime DataDomani { get; set; } = DateTime.Today.AddDays(1);
        public DateTime? GiornoSelezionato { get; set; }

        public List<NaveDetailViewModel> NaviOggi { get; set; }
        public List<NaveDetailViewModel> NaviDomani { get; set; }
        public List<NaveDetailViewModel> NaviGiornoSelezionato { get; set; }
        public List<NaveDetailViewModel> TutteLeNavi { get; set; }

        // Giorni futuri (da dopodomani in poi) che hanno navi
        public List<GiornoConNaviViewModel> GiorniFuturiConNavi { get; set; }

        // Genera la lista dei prossimi 14 giorni con info sulle navi
        public List<GiornoTimelineViewModel> GetTimeline()
        {
            var timeline = new List<GiornoTimelineViewModel>();
            var oggi = DateTime.Today;

            // Mostra i prossimi 14 giorni (esclusi oggi e domani che sono già nelle tabelle)
            for (int i = 2; i <= 14; i++)
            {
                var giorno = oggi.AddDays(i);
                var naviDelGiorno = TutteLeNavi.Where(n => n.DataArrivo.Date == giorno.Date).ToList();

                timeline.Add(new GiornoTimelineViewModel
                {
                    Data = giorno,
                    NumeroNavi = naviDelGiorno.Count,
                    HasNavi = naviDelGiorno.Any()
                });
            }

            return timeline;
        }

        // Metodo per ottenere le fasce orarie occupate per un dato giorno
        public List<int> GetFasceOccupate(DateTime data)
        {
            var fasceOccupate = new List<int>();
            var naviDelGiorno = TutteLeNavi.Where(n => n.DataArrivo.Date == data.Date).ToList();

            foreach (var nave in naviDelGiorno)
            {
                if (nave.FasciaMattina) fasceOccupate.Add(0);
                if (nave.FasciaPomeriggio) fasceOccupate.Add(1);
                if (nave.FasciaSera) fasceOccupate.Add(2);
            }

            return fasceOccupate.Distinct().ToList();
        }

        // Verifica se una fascia è disponibile per un dato giorno (escludendo una nave specifica)
        public bool IsFasciaDisponibile(DateTime data, int fascia, int? escludiNaveId = null)
        {
            var naviDelGiorno = TutteLeNavi
                .Where(n => n.DataArrivo.Date == data.Date && (!escludiNaveId.HasValue || n.Id != escludiNaveId.Value))
                .ToList();

            foreach (var nave in naviDelGiorno)
            {
                if (fascia == 0 && nave.FasciaMattina) return false;
                if (fascia == 1 && nave.FasciaPomeriggio) return false;
                if (fascia == 2 && nave.FasciaSera) return false;
            }

            return true;
        }
    }

    public class NaveDetailViewModel
    {
        public NaveDetailViewModel()
        {
            DataArrivo = DateTime.Today;
        }

        public int Id { get; set; }

        [Display(Name = "Nome Nave")]
        [Required(ErrorMessage = "Il nome della nave è obbligatorio")]
        public string Nome { get; set; }

        [Display(Name = "Tipo Nave")]
        public TipoNave Tipo { get; set; }

        [Display(Name = "Data Arrivo")]
        [Required(ErrorMessage = "La data di arrivo è obbligatoria")]
        public DateTime DataArrivo { get; set; }

        [Display(Name = "Pontile")]
        [Required(ErrorMessage = "Il numero del pontile è obbligatorio")]
        public int? Pontile { get; set; }

        // Fasce orarie (una nave può occupare più fasce)
        [Display(Name = "00:00 / 08:00")]
        public bool FasciaMattina { get; set; }

        [Display(Name = "08:00 / 16:00")]
        public bool FasciaPomeriggio { get; set; }

        [Display(Name = "16:00 / 24:00")]
        public bool FasciaSera { get; set; }

        [Display(Name = "Richiede Gruisti")]
        public bool RichiedeGruisti { get; set; }

        [Display(Name = "Richiede Mulettisti")]
        public bool RichiedeMulettisti { get; set; }

        // Proprietà per visualizzazione
        public string TipoDescrizione => Tipo switch
        {
            TipoNave.Container => "Container",
            TipoNave.Portarinfuse => "Portarinfuse",
            TipoNave.NaveTraghetto => "Nave Traghetto",
            TipoNave.Petroliera => "Petroliera",
            TipoNave.CargoPronta => "Cargo Pronta",
            _ => ""
        };

        public string RuoliRichiesti
        {
            get
            {
                var ruoli = new List<string>();
                if (RichiedeGruisti) ruoli.Add("Gruisti");
                if (RichiedeMulettisti) ruoli.Add("Mulettisti");
                return ruoli.Count > 0 ? string.Join("\n", ruoli) : "-";
            }
        }

        public bool HasAlmenoUnaFascia => FasciaMattina || FasciaPomeriggio || FasciaSera;

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

    public static class TipiNave
    {
        public static List<(TipoNave Tipo, string Nome)> GetTipi()
        {
            return new List<(TipoNave, string)>
            {
                (TipoNave.Container, "Container"),
                (TipoNave.Portarinfuse, "Portarinfuse"),
                (TipoNave.NaveTraghetto, "Nave Traghetto"),
                (TipoNave.Petroliera, "Petroliera"),
                (TipoNave.CargoPronta, "Cargo Pronta")
            };
        }
    }

    // Classe per i giorni con navi nella timeline
    public class GiornoConNaviViewModel
    {
        public DateTime Data { get; set; }
        public List<NaveDetailViewModel> Navi { get; set; } = new List<NaveDetailViewModel>();
    }

    // Classe per la timeline dei giorni futuri
    public class GiornoTimelineViewModel
    {
        public DateTime Data { get; set; }
        public int NumeroNavi { get; set; }
        public bool HasNavi { get; set; }

        public string GiornoFormattato => Data.ToString("dd");
        public string MeseFormattato => Data.ToString("MMM");
        public string DataCompleta => Data.ToString("dd/MM/yyyy");
    }
}
