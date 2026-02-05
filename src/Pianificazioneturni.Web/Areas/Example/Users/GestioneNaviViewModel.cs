using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Pianificazioneturni.Web.Areas.Example.Users
{
    //Enum per il tipo di nave
    public enum TipoNave
    {
        Container,
        Portarinfuse,
        NaveTraghetto,
        Petroliera,
        Cargo
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

        //giorni futuri (da dopodomani in poi)
        public List<GiornoConNaviViewModel> GiorniFuturiConNavi { get; set; }

        //genera timeline 7 giorni a partire da oggi
        public List<GiornoTimelineViewModel> GetTimeline()
        {
            var timeline = new List<GiornoTimelineViewModel>();
            var oggi = DateTime.Today;

            for (int i = 0; i < 7; i++)
            {
                var giorno = oggi.AddDays(i);
                var naviDelGiorno = TutteLeNavi.Where(n => n.DatePresenza.Any(d => d.Date == giorno.Date)).ToList();

                timeline.Add(new GiornoTimelineViewModel
                {
                    Data = giorno,
                    NumeroNavi = naviDelGiorno.Count,
                    HasNavi = naviDelGiorno.Any()
                });
            }

            return timeline;
        }

        //restituisce le navi presenti in un dato giorno
        public List<NaveDetailViewModel> GetNaviPerGiorno(DateTime data)
        {
            return TutteLeNavi.Where(n => n.DatePresenza.Any(d => d.Date == data.Date)).ToList();
        }

        //controlla se una fascia è occupata in una certa data
        public bool IsFasciaOccupata(DateTime data, int fascia, int? escludiNaveId = null)
        {
            var naviDelGiorno = TutteLeNavi
                .Where(n => n.DatePresenza.Any(d => d.Date == data.Date) && (!escludiNaveId.HasValue || n.Id != escludiNaveId.Value))
                .ToList();

            foreach (var nave in naviDelGiorno)
            {
                if (nave.HasFasciaInData(data, fascia)) return true;
            }

            return false;
        }
    }

    public class NaveDetailViewModel
    {
        public NaveDetailViewModel()
        {
            DatePresenza = new List<DateTime>();
            FascePerData = new Dictionary<string, List<int>>();
        }

        public int Id { get; set; }

        [Display(Name = "Nome Nave")]
        [Required(ErrorMessage = "Il nome della nave è obbligatorio")]
        public string Nome { get; set; }

        [Display(Name = "Tipo Nave")]
        public TipoNave Tipo { get; set; }

        [Display(Name = "Date Presenza")]
        public List<DateTime> DatePresenza { get; set; }

        //fasce orarie per ogni data (chiave: yyyy-MM-dd, valore: lista fasce 0=mattina, 1=pomeriggio, 2=sera)
        public Dictionary<string, List<int>> FascePerData { get; set; }

        //campi arrivo/partenza (stile Booking.com)
        public DateTime? DataArrivo { get; set; }
        public int OrarioArrivo { get; set; } // 0, 8, 16
        public DateTime? DataPartenza { get; set; }
        public int OrarioPartenza { get; set; } // 8, 16, 24

        [Display(Name = "Pontile")]
        [Required(ErrorMessage = "Il numero del pontile è obbligatorio")]
        public int? Pontile { get; set; }

        //helper methods per verificare se una fascia è presente in una specifica data
        public bool HasFasciaInData(DateTime data, int fascia)
        {
            var dataKey = data.ToString("yyyy-MM-dd");
            return FascePerData.ContainsKey(dataKey) && FascePerData[dataKey].Contains(fascia);
        }

        /// <summary>
        /// Calcola DatePresenza e FascePerData a partire da DataArrivo/OrarioArrivo e DataPartenza/OrarioPartenza.
        /// Fascia 0=Mattina(00-08), 1=Pomeriggio(08-16), 2=Sera(16-24)
        /// OrarioArrivo: 0→fascia 0, 8→fascia 1, 16→fascia 2
        /// OrarioPartenza: 8→fascia 0, 16→fascia 1, 24→fascia 2
        /// </summary>
        public void CalcolaDateEFasce()
        {
            DatePresenza = new List<DateTime>();
            FascePerData = new Dictionary<string, List<int>>();

            if (!DataArrivo.HasValue || !DataPartenza.HasValue) return;

            // Converte orario arrivo in indice fascia di partenza (da quale fascia inizia)
            int fasciaInizio = OrarioArrivo switch { 0 => 0, 8 => 1, 16 => 2, _ => 0 };
            // Converte orario partenza in indice fascia finale (fino a quale fascia inclusa)
            int fasciaFine = OrarioPartenza switch { 8 => 0, 16 => 1, 24 => 2, _ => 2 };

            var dataCorrente = DataArrivo.Value.Date;
            var dataFine = DataPartenza.Value.Date;

            while (dataCorrente <= dataFine)
            {
                var dataKey = dataCorrente.ToString("yyyy-MM-dd");
                var fasce = new List<int>();

                int inizio = (dataCorrente == DataArrivo.Value.Date) ? fasciaInizio : 0;
                int fine = (dataCorrente == DataPartenza.Value.Date) ? fasciaFine : 2;

                for (int f = inizio; f <= fine; f++)
                {
                    fasce.Add(f);
                }

                if (fasce.Any())
                {
                    DatePresenza.Add(dataCorrente);
                    FascePerData[dataKey] = fasce;
                }

                dataCorrente = dataCorrente.AddDays(1);
            }
        }

        [Display(Name = "Richiede Gruisti")]
        public bool RichiedeGruisti { get; set; }

        [Display(Name = "Richiede Mulettisti")]
        public bool RichiedeMulettisti { get; set; }

        public string TipoDescrizione => Tipo switch
        {
            TipoNave.Container => "Container",
            TipoNave.Portarinfuse => "Portarinfuse",
            TipoNave.NaveTraghetto => "Nave Traghetto",
            TipoNave.Petroliera => "Petroliera",
            TipoNave.Cargo => "Cargo",
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

        public bool HasAlmenoUnaFascia => FascePerData.Any(kvp => kvp.Value.Any());

        public string Colore { get; set; }
    }

    public static class ColoriNavi
    {
        private static readonly List<string> _colori = new List<string>
        {
            "#FF6B6B", 
            "#4ECDC4", 
            "#45B7D1", 
            "#96CEB4", 
            "#FFEAA7", 
            "#DDA0DD", 
            "#98D8C8", 
            "#F7DC6F", 
            "#BB8FCE", 
            "#85C1E9" 
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
                (TipoNave.Cargo, "Cargo")
            };
        }
    }

    //classe per i giorni con navi nella timeline
    public class GiornoConNaviViewModel
    {
        public DateTime Data { get; set; }
        public List<NaveDetailViewModel> Navi { get; set; } = new List<NaveDetailViewModel>();
    }

    //classe per la timeline dei giorni futuri
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
