using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Pianificazioneturni.Web.Infrastructure;
using Pianificazioneturni.Web.SignalR;
using Pianificazioneturni.Web.SignalR.Hubs.Events;
using PianificazioneTurni.Services.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pianificazioneturni.Web.Areas.Example.Users
{
    [Area("Example")]
    public partial class UsersController : AuthenticatedBaseController
    {
        private readonly SharedService _sharedService;
        private readonly IPublishDomainEvents _publisher;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        //percorsi file JSON per la persistenza dei dati
        private static readonly string DataFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        private static readonly string NaviFilePath = Path.Combine(DataFolder, "navi.json");
        private static readonly string DipendentiFilePath = Path.Combine(DataFolder, "dipendenti.json");
        private static readonly string AssegnazioniFilePath = Path.Combine(DataFolder, "assegnazioni.json");
        private static readonly string NextNaveIdFilePath = Path.Combine(DataFolder, "next_nave_id.json");

        //lista statica dipendenti (simula database)
        private static List<DipendenteDetailViewModel> _dipendenti = LoadDipendenti();

        //lista statica navi (simula database)
        private static List<NaveDetailViewModel> _navi = LoadNavi();
        private static int _nextNaveId = LoadNextNaveId();

        private static List<NaveDetailViewModel> InitNavi()
        {
            return new List<NaveDetailViewModel>();
        }

        private static List<DipendenteDetailViewModel> InitDipendenti()
        {
            return new List<DipendenteDetailViewModel>
            {
                new DipendenteDetailViewModel { Id = 1, Nome = "Rossi Mario", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2027, 6, 15) },
                new DipendenteDetailViewModel { Id = 2, Nome = "Blu Marco", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2027, 8, 20) },
                new DipendenteDetailViewModel { Id = 3, Nome = "Bianchi Filippo", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2026, 1, 10) }, 
                new DipendenteDetailViewModel { Id = 4, Nome = "Cortesi Giulia", Ruolo = "Addetto terminal", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 5, Nome = "Gialli Monica", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2027, 3, 20) },
                new DipendenteDetailViewModel { Id = 6, Nome = "Verdi Luca", Ruolo = "Ormeggiatore", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 7, Nome = "Azzurri Margherita", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2027, 9, 10) },
                new DipendenteDetailViewModel { Id = 8, Nome = "Viola Riccardo", Ruolo = "Addetto alla Sicurezza", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 9, Nome = "Arancioni Sofia", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2027, 11, 30) },
                new DipendenteDetailViewModel { Id = 10, Nome = "Celeste Lorenzo", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2027, 8, 25) },
                new DipendenteDetailViewModel { Id = 11, Nome = "Rosa Alex", Ruolo = "Addetto terminal", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 12, Nome = "Neri Federico", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2025, 12, 5) }, 
                new DipendenteDetailViewModel { Id = 13, Nome = "Marroni Eleonora", Ruolo = "Ormeggiatore", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 14, Nome = "Grigi Roberto", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2027, 4, 18) },
                new DipendenteDetailViewModel { Id = 15, Nome = "Lavanda Francesco", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2027, 5, 12) },
                new DipendenteDetailViewModel { Id = 16, Nome = "Giannini Matteo", Ruolo = "Addetto alla Sicurezza", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 17, Nome = "Forcellini Filippo", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2027, 7, 22) },
                new DipendenteDetailViewModel { Id = 18, Nome = "Limoni Marta", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2027, 2, 14) },
                new DipendenteDetailViewModel { Id = 19, Nome = "Acqua Filomena", Ruolo = "Addetto terminal", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 20, Nome = "Fuochi Davide", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2027, 10, 8) },
                new DipendenteDetailViewModel { Id = 21, Nome = "Lampone Federica", Ruolo = "Ormeggiatore", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 22, Nome = "Agnelli Lucia", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2027, 3, 30) },
                new DipendenteDetailViewModel { Id = 23, Nome = "Rinaldi Martina", Ruolo = "Addetto alla Sicurezza", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 24, Nome = "Tonelli Alessandro", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2027, 7, 19) },
                new DipendenteDetailViewModel { Id = 25, Nome = "Nardelli Tommaso", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2027, 12, 25) }
            };
        }

        //Assegnazioni dipendenti alle navi
        private static Dictionary<string, List<int>> _assegnazioniDipendenti = LoadAssegnazioni();

        #region Persistenza Dati JSON

        private static void EnsureDataFolderExists()
        {
            if (!Directory.Exists(DataFolder))
            {
                Directory.CreateDirectory(DataFolder);
            }
        }

        private static List<NaveDetailViewModel> LoadNavi()
        {
            try
            {
                EnsureDataFolderExists();
                if (System.IO.File.Exists(NaviFilePath))
                {
                    var json = System.IO.File.ReadAllText(NaviFilePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var options = new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        return System.Text.Json.JsonSerializer.Deserialize<List<NaveDetailViewModel>>(json, options) ?? InitNavi();
                    }
                }
            }
            catch
            {
                try { System.IO.File.Delete(NaviFilePath); } catch { }
            }
            return InitNavi();
        }

        private static void SaveNavi()
        {
            try
            {
                EnsureDataFolderExists();
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };
                var json = System.Text.Json.JsonSerializer.Serialize(_navi, options);
                System.IO.File.WriteAllText(NaviFilePath, json);
            }
            catch
            {   
                
            }
        }

        private static List<DipendenteDetailViewModel> LoadDipendenti()
        {
            try
            {
                EnsureDataFolderExists();
                if (System.IO.File.Exists(DipendentiFilePath))
                {
                    var json = System.IO.File.ReadAllText(DipendentiFilePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var options = new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        return System.Text.Json.JsonSerializer.Deserialize<List<DipendenteDetailViewModel>>(json, options) ?? InitDipendenti();
                    }
                }
            }
            catch
            {
                try { System.IO.File.Delete(DipendentiFilePath); } catch { }
            }
            return InitDipendenti();
        }

        private static void SaveDipendenti()
        {
            try
            {
                EnsureDataFolderExists();
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };
                var json = System.Text.Json.JsonSerializer.Serialize(_dipendenti, options);
                System.IO.File.WriteAllText(DipendentiFilePath, json);
            }
            catch
            {

            }
        }

        private static Dictionary<string, List<int>> LoadAssegnazioni()
        {
            try
            {
                EnsureDataFolderExists();
                if (System.IO.File.Exists(AssegnazioniFilePath))
                {
                    var json = System.IO.File.ReadAllText(AssegnazioniFilePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var options = new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<int>>>(json, options) ?? InitAssegnazioni();
                    }
                }
            }
            catch
            {
                try { System.IO.File.Delete(AssegnazioniFilePath); } catch { }
            }
            return InitAssegnazioni();
        }

        private static void SaveAssegnazioni()
        {
            try
            {
                EnsureDataFolderExists();
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };
                var json = System.Text.Json.JsonSerializer.Serialize(_assegnazioniDipendenti, options);
                System.IO.File.WriteAllText(AssegnazioniFilePath, json);
            }
            catch
            {
               
            }
        }


        private static int LoadNextNaveId()
        {
            try
            {
                EnsureDataFolderExists();
                if (System.IO.File.Exists(NextNaveIdFilePath))
                {
                    var json = System.IO.File.ReadAllText(NextNaveIdFilePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        return System.Text.Json.JsonSerializer.Deserialize<int>(json);
                    }
                }
            }
            catch
            {
                try { System.IO.File.Delete(NextNaveIdFilePath); } catch { }
            }
            return 4;
        }

        private static void SaveNextNaveId()
        {
            try
            {
                EnsureDataFolderExists();
                var json = System.Text.Json.JsonSerializer.Serialize(_nextNaveId);
                System.IO.File.WriteAllText(NextNaveIdFilePath, json);
            }
            catch { }
        }

        #endregion

        private static Dictionary<string, List<int>> InitAssegnazioni()
        {
            return new Dictionary<string, List<int>>();
        }

        public UsersController(SharedService sharedService, IPublishDomainEvents publisher, IStringLocalizer<SharedResource> sharedLocalizer)
        {
            _sharedService = sharedService;
            _publisher = publisher;
            _sharedLocalizer = sharedLocalizer;

            ModelUnbinderHelpers.ModelUnbinders.Add(typeof(IndexViewModel), new SimplePropertyModelUnbinder());
        }

        [HttpGet]
        public virtual async Task<IActionResult> Index(IndexViewModel model)
        {
            var users = await _sharedService.Query(model.ToUsersIndexQuery());
            model.SetUsers(users);

            //popola dati in Pianificazione Turni
            CaricaDatiPianificazione(model);

            return View(model);
        }

        private void CaricaDatiPianificazione(IndexViewModel model)
        {
            var oggi = DateTime.Today;
            var domani = DateTime.Today.AddDays(1);
            var random = new Random();

            //converte dipendenti da Gestione Dipendenti a DipendenteViewModel
            model.TuttiDipendenti = _dipendenti.Select(d => new DipendenteViewModel
            {
                Id = d.Id,
                Nome = d.Nome.Split(' ').Length > 1 ? d.Nome.Split(' ')[1] : d.Nome,
                Cognome = d.Nome.Split(' ')[0],
                Ruolo = d.Ruolo,
                PatenteScaduta = d.Patente && d.Scadenza.HasValue && d.Scadenza.Value < DateTime.Today
            }).ToList();

            //filtra gruisti e mulettisti disponibili
            model.Gruisti = model.TuttiDipendenti.Where(d => d.Ruolo == "Gruista").ToList();
            model.Mulettisti = model.TuttiDipendenti.Where(d => d.Ruolo == "Mulettista").ToList();
            model.AddettiTerminal = model.TuttiDipendenti.Where(d => d.Ruolo == "Addetto terminal").ToList();
            model.Ormeggiatori = model.TuttiDipendenti.Where(d => d.Ruolo == "Ormeggiatore").ToList();
            model.AddettiSicurezza = model.TuttiDipendenti.Where(d => d.Ruolo == "Addetto alla Sicurezza").ToList();

            //Navi oggi da Gestione Navi
            var naviOggiDb = _navi.Where(n => n.DatePresenza.Any(d => d.Date == oggi)).ToList();
            model.NaviOggi = naviOggiDb.Select(n => CreaNaveViewModel(n, model.TuttiDipendenti, random, oggi)).ToList();

            //Navi domani da Gestione Navi
            var naviDomaniDb = _navi.Where(n => n.DatePresenza.Any(d => d.Date == domani)).ToList();
            model.NaviDomani = naviDomaniDb.Select(n => CreaNaveViewModel(n, model.TuttiDipendenti, random, domani)).ToList();

            model.OraCorrente = DateTime.Now.Hour;

            //Passa le assegnazioni per filtrare i dipendenti disponibili
            //Converte le chiavi per includere il giorno (oggi=0, domani=1)
            model.Assegnazioni = new Dictionary<string, List<int>>();
            foreach (var nave in model.NaviOggi)
            {
                if (nave.DipendentiMattina.Any())
                    model.Assegnazioni[$"{nave.Id}_0_oggi"] = nave.DipendentiMattina.Select(d => d.Id).ToList();
                if (nave.DipendentiPomeriggio.Any())
                    model.Assegnazioni[$"{nave.Id}_1_oggi"] = nave.DipendentiPomeriggio.Select(d => d.Id).ToList();
                if (nave.DipendentiSera.Any())
                    model.Assegnazioni[$"{nave.Id}_2_oggi"] = nave.DipendentiSera.Select(d => d.Id).ToList();
            }
            foreach (var nave in model.NaviDomani)
            {
                if (nave.DipendentiMattina.Any())
                    model.Assegnazioni[$"{nave.Id}_0_domani"] = nave.DipendentiMattina.Select(d => d.Id).ToList();
                if (nave.DipendentiPomeriggio.Any())
                    model.Assegnazioni[$"{nave.Id}_1_domani"] = nave.DipendentiPomeriggio.Select(d => d.Id).ToList();
                if (nave.DipendentiSera.Any())
                    model.Assegnazioni[$"{nave.Id}_2_domani"] = nave.DipendentiSera.Select(d => d.Id).ToList();
            }

        }

        private NaveViewModel CreaNaveViewModel(NaveDetailViewModel naveDb, List<DipendenteViewModel> tuttiDipendenti, Random random, DateTime data)
        {
            var nave = new NaveViewModel
            {
                Id = naveDb.Id,
                Nome = naveDb.Nome,
                Pontile = naveDb.Pontile ?? 0,
                FasciaMattina = naveDb.HasFasciaInData(data, 0),
                FasciaPomeriggio = naveDb.HasFasciaInData(data, 1),
                FasciaSera = naveDb.HasFasciaInData(data, 2),
                RichiedeGruisti = naveDb.RichiedeGruisti,
                RichiedeMulettisti = naveDb.RichiedeMulettisti,
                RichiedeAddettiTerminal = naveDb.RichiedeAddettiTerminal,
                RichiedeOrmeggiatori = naveDb.RichiedeOrmeggiatori,
                RichiedeAddettiSicurezza = naveDb.RichiedeAddettiSicurezza
            };

            //Carica dipendenti salvati
            nave.DipendentiMattina = GetDipendentiAssegnati(naveDb.Id, 0, tuttiDipendenti);
            nave.DipendentiPomeriggio = GetDipendentiAssegnati(naveDb.Id, 1, tuttiDipendenti);
            nave.DipendentiSera = GetDipendentiAssegnati(naveDb.Id, 2, tuttiDipendenti);

            return nave;
        }

        private List<DipendenteViewModel> GetDipendentiAssegnati(int naveId, int fascia, List<DipendenteViewModel> tuttiDipendenti)
        {
            var chiave = $"{naveId}_{fascia}";
            if (_assegnazioniDipendenti.TryGetValue(chiave, out var idDipendenti))
            {
                //Mantiene l'ordine degli ID in idDipendenti
                return idDipendenti
                    .Select(id => tuttiDipendenti.FirstOrDefault(d => d.Id == id))
                    .Where(d => d != null)
                    .ToList();
            }
            return new List<DipendenteViewModel>();
        }

        [HttpPost]
        public virtual IActionResult SalvaAssegnazione(int naveId, int fascia, [FromBody] List<int> dipendentiIds)
        {
            var chiave = $"{naveId}_{fascia}";
            _assegnazioniDipendenti[chiave] = dipendentiIds ?? new List<int>();
            SaveAssegnazioni();
            return Json(new { success = true });
        }

        [HttpPost]
        public virtual IActionResult CambiaDipendente(int naveId, int fascia, int vecchioDipendenteId, int nuovoDipendenteId)
        {
            var chiave = $"{naveId}_{fascia}";
            if (_assegnazioniDipendenti.TryGetValue(chiave, out var idDipendenti))
            {
                var index = idDipendenti.IndexOf(vecchioDipendenteId);
                if (index >= 0)
                {
                    idDipendenti[index] = nuovoDipendenteId;
                    SaveAssegnazioni();
                }
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public virtual IActionResult EliminaDipendente(int naveId, int fascia, int dipendenteId)
        {
            var chiave = $"{naveId}_{fascia}";
            if (_assegnazioniDipendenti.TryGetValue(chiave, out var idDipendenti))
            {
                idDipendenti.Remove(dipendenteId);
                SaveAssegnazioni();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public virtual IActionResult EliminaTurno(int naveId, int fascia)
        {
            var chiave = $"{naveId}_{fascia}";
            if (_assegnazioniDipendenti.ContainsKey(chiave))
            {
                _assegnazioniDipendenti[chiave] = new List<int>();
                SaveAssegnazioni();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        [HttpGet]
        public virtual IActionResult GestioneNavi(string giorno = null)
        {
            var oggi = DateTime.Today;
            var domani = DateTime.Today.AddDays(1);
            DateTime? giornoSelezionato = null;

            if (!string.IsNullOrEmpty(giorno) && DateTime.TryParse(giorno, out var dataParsata))
            {
                if (dataParsata.Date > domani.Date)
                {
                    giornoSelezionato = dataParsata.Date;
                }
            }

            if (_navi == null)
            {
                _navi = InitNavi();
            }

            var model = new GestioneNaviViewModel
            {
                DataOggi = oggi,
                DataDomani = domani,
                GiornoSelezionato = giornoSelezionato,
                TutteLeNavi = _navi ?? new List<NaveDetailViewModel>(),
                NaviOggi = _navi.Where(n => n.DatePresenza.Any(d => d.Date == oggi)).ToList(),
                NaviDomani = _navi.Where(n => n.DatePresenza.Any(d => d.Date == domani)).ToList(),
                NaviGiornoSelezionato = giornoSelezionato.HasValue
                    ? _navi.Where(n => n.DatePresenza.Any(d => d.Date == giornoSelezionato.Value)).ToList()
                    : null
            };
            return View("Gestione_Navi", model);
        }

        [HttpGet]
        public virtual IActionResult DettaglioNave(int? id)
        {
            NaveDetailViewModel nave;

            if (id.HasValue)
            {
                nave = _navi.FirstOrDefault(n => n.Id == id.Value);
                if (nave == null)
                {
                    return NotFound();
                }
            }
            else
            {
                //nuova nave
                nave = new NaveDetailViewModel
                {
                    Id = 0,
                    DatePresenza = new List<DateTime>(),
                    Tipo = TipoNave.Container
                };
            }

            return Json(new
            {
                id = nave.Id,
                nome = nave.Nome ?? "",
                pontile = nave.Pontile ?? 1,
                richiedeGruisti = nave.RichiedeGruisti,
                richiedeMulettisti = nave.RichiedeMulettisti,
                richiedeAddettiTerminal = nave.RichiedeAddettiTerminal,
                richiedeOrmeggiatori = nave.RichiedeOrmeggiatori,
                richiedeAddettiSicurezza = nave.RichiedeAddettiSicurezza,
                dataArrivo = nave.DataArrivo?.ToString("yyyy-MM-dd") ?? "",
                orarioArrivo = nave.OrarioArrivo,
                dataPartenza = nave.DataPartenza?.ToString("yyyy-MM-dd") ?? "",
                orarioPartenza = nave.OrarioPartenza
            });
        }

        [HttpPost]
        public virtual IActionResult SalvaNave(int id, string nome, int? pontile,
            bool richiedeGruisti, bool richiedeMulettisti, bool richiedeAddettiTerminal, bool richiedeOrmeggiatori, bool richiedeAddettiSicurezza,
            string dataArrivo, int orarioArrivo, string dataPartenza, int orarioPartenza)
        {
            //parse date arrivo/partenza
            if (!DateTime.TryParse(dataArrivo, out var parsedDataArrivo) || !DateTime.TryParse(dataPartenza, out var parsedDataPartenza))
            {
                Alerts.AddError(this, "Date arrivo/partenza non valide");
                return RedirectToAction(Actions.GestioneNavi());
            }

            //validazione: partenza >= arrivo
            if (parsedDataPartenza < parsedDataArrivo || (parsedDataPartenza == parsedDataArrivo && orarioPartenza <= orarioArrivo))
            {
                Alerts.AddError(this, "La data/orario di partenza deve essere successiva all'arrivo");
                return RedirectToAction(Actions.GestioneNavi());
            }

            //calcola DatePresenza e FascePerData dal range arrivo/partenza
            var naveCalc = new NaveDetailViewModel
            {
                DataArrivo = parsedDataArrivo,
                OrarioArrivo = orarioArrivo,
                DataPartenza = parsedDataPartenza,
                OrarioPartenza = orarioPartenza
            };
            naveCalc.CalcolaDateEFasce();

            if (naveCalc.DatePresenza.Count == 0)
            {
                Alerts.AddError(this, "Nessun giorno di presenza calcolato");
                return RedirectToAction(Actions.GestioneNavi());
            }

            if (id == 0)
            {
                var nuovaNave = new NaveDetailViewModel
                {
                    Id = _nextNaveId++,
                    Nome = nome,
                    Tipo = TipoNave.Container,
                    DatePresenza = naveCalc.DatePresenza,
                    FascePerData = naveCalc.FascePerData,
                    DataArrivo = parsedDataArrivo,
                    OrarioArrivo = orarioArrivo,
                    DataPartenza = parsedDataPartenza,
                    OrarioPartenza = orarioPartenza,
                    Pontile = pontile,
                    RichiedeGruisti = richiedeGruisti,
                    RichiedeMulettisti = richiedeMulettisti,
                    RichiedeAddettiTerminal = richiedeAddettiTerminal,
                    RichiedeOrmeggiatori = richiedeOrmeggiatori,
                    RichiedeAddettiSicurezza = richiedeAddettiSicurezza,
                    Colore = ColoriNavi.GetColore(_navi.Count)
                };
                _navi.Add(nuovaNave);
                SaveNavi();
                SaveNextNaveId();
                Alerts.AddSuccess(this, "Nave aggiunta con successo");
            }
            else
            {
                //Modifica nave esistente
                var nave = _navi.FirstOrDefault(n => n.Id == id);
                if (nave != null)
                {
                    nave.Nome = nome;
                    nave.DatePresenza = naveCalc.DatePresenza;
                    nave.FascePerData = naveCalc.FascePerData;
                    nave.DataArrivo = parsedDataArrivo;
                    nave.OrarioArrivo = orarioArrivo;
                    nave.DataPartenza = parsedDataPartenza;
                    nave.OrarioPartenza = orarioPartenza;
                    nave.Pontile = pontile;
                    nave.RichiedeGruisti = richiedeGruisti;
                    nave.RichiedeMulettisti = richiedeMulettisti;
                    nave.RichiedeAddettiTerminal = richiedeAddettiTerminal;
                    nave.RichiedeOrmeggiatori = richiedeOrmeggiatori;
                    nave.RichiedeAddettiSicurezza = richiedeAddettiSicurezza;
                    SaveNavi();
                    Alerts.AddSuccess(this, "Nave aggiornata con successo");
                }
            }

            return RedirectToAction(Actions.GestioneNavi());
        }

        [HttpPost]
        public virtual IActionResult EliminaNave(int id)
        {
            var nave = _navi.FirstOrDefault(n => n.Id == id);
            if (nave != null)
            {
                _navi.Remove(nave);
                SaveNavi();
                Alerts.AddSuccess(this, "Nave eliminata con successo");
            }

            return RedirectToAction(Actions.GestioneNavi());
        }


        [HttpGet]
        public virtual IActionResult GestioneDipendenti(bool filtroPatentiScadute = false)
        {
            var model = new GestioneDipendentiViewModel
            {
                Dipendenti = _dipendenti,
                FiltroPatentiScadute = filtroPatentiScadute
            };

            return View("Gestione_Dipendenti", model);
        }

        [HttpGet]
        public virtual IActionResult DettaglioDipendente(int id)
        {
            var dipendente = _dipendenti.FirstOrDefault(d => d.Id == id);
            if (dipendente == null)
            {
                return NotFound();
            }

            ViewBag.Ruoli = RuoliDipendente.GetRuoli();
            return PartialView("_DettaglioDipendente", dipendente);
        }

        [HttpPost]
        public virtual IActionResult SalvaDipendente(DipendenteDetailViewModel model)
        {
            var dipendente = _dipendenti.FirstOrDefault(d => d.Id == model.Id);
            if (dipendente == null)
            {
                return NotFound();
            }

            dipendente.Ruolo = model.Ruolo;

            //solo per Gruista e Mulettista si gestisce patente e scadenza
            if (dipendente.RichiedePatente)
            {
                dipendente.Patente = model.Patente;
                dipendente.Scadenza = model.Scadenza;
            }
            else
            {
                dipendente.Patente = false;
                dipendente.Scadenza = null;
            }

            SaveDipendenti();
            Alerts.AddSuccess(this, "Dipendente aggiornato con successo");
            return RedirectToAction(Actions.GestioneDipendenti());
        }
    }
}
