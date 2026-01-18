using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Pianificazioneturni.Web.Infrastructure;
using Pianificazioneturni.Web.SignalR;
using Pianificazioneturni.Web.SignalR.Hubs.Events;
using PianificazioneTurni.Services.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pianificazioneturni.Web.Areas.Example.Users
{
    [Area("Example")]
    public partial class UsersController : AuthenticatedBaseController
    {
        private readonly SharedService _sharedService;
        private readonly IPublishDomainEvents _publisher;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        // Lista statica dipendenti (simula database)
        private static List<DipendenteDetailViewModel> _dipendenti = InitDipendenti();

        // Lista statica navi (simula database)
        private static List<NaveDetailViewModel> _navi = InitNavi();
        private static int _nextNaveId = 4;

        private static List<NaveDetailViewModel> InitNavi()
        {
            return new List<NaveDetailViewModel>
            {
                new NaveDetailViewModel
                {
                    Id = 1,
                    Nome = "Nave 1",
                    Tipo = TipoNave.Container,
                    Pontile = 10,
                    DataArrivo = DateTime.Today,
                    FasciaMattina = true,
                    FasciaPomeriggio = true,
                    FasciaSera = false,
                    RichiedeGruisti = true,
                    RichiedeMulettisti = true,
                    Colore = ColoriNavi.GetColore(0)
                },
                new NaveDetailViewModel
                {
                    Id = 2,
                    Nome = "Nave 2",
                    Tipo = TipoNave.Portarinfuse,
                    Pontile = 30,
                    DataArrivo = DateTime.Today,
                    FasciaMattina = false,
                    FasciaPomeriggio = false,
                    FasciaSera = true,
                    RichiedeGruisti = false,
                    RichiedeMulettisti = true,
                    Colore = ColoriNavi.GetColore(1)
                },
                new NaveDetailViewModel
                {
                    Id = 3,
                    Nome = "Nave 3",
                    Tipo = TipoNave.NaveTraghetto,
                    Pontile = 20,
                    DataArrivo = DateTime.Today.AddDays(1),
                    FasciaMattina = true,
                    FasciaPomeriggio = true,
                    FasciaSera = true,
                    RichiedeGruisti = false,
                    RichiedeMulettisti = true,
                    Colore = ColoriNavi.GetColore(2)
                }
            };
        }

        private static List<DipendenteDetailViewModel> InitDipendenti()
        {
            return new List<DipendenteDetailViewModel>
            {
                new DipendenteDetailViewModel { Id = 1, Nome = "Rossi Mario", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2027, 6, 15) },
                new DipendenteDetailViewModel { Id = 2, Nome = "Blu Marco", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2027, 8, 20) },
                new DipendenteDetailViewModel { Id = 3, Nome = "Bianchi Filippo", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2026, 1, 10) }, // Scaduta
                new DipendenteDetailViewModel { Id = 4, Nome = "Cortesi Giulia", Ruolo = "Addetto terminal", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 5, Nome = "Gialli Monica", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2027, 3, 20) },
                new DipendenteDetailViewModel { Id = 6, Nome = "Verdi Luca", Ruolo = "Ormeggiatore", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 7, Nome = "Azzurri Margherita", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2027, 9, 10) },
                new DipendenteDetailViewModel { Id = 8, Nome = "Viola Riccardo", Ruolo = "Addetto alla Sicurezza", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 9, Nome = "Arancioni Sofia", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2027, 11, 30) },
                new DipendenteDetailViewModel { Id = 10, Nome = "Celeste Lorenzo", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2027, 8, 25) },
                new DipendenteDetailViewModel { Id = 11, Nome = "Rosa Alex", Ruolo = "Addetto terminal", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 12, Nome = "Neri Federico", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2025, 12, 5) }, // Scaduta
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

        // Assegnazioni dipendenti alle navi (chiave: naveId_fascia, valore: lista id dipendenti)
        private static Dictionary<string, List<int>> _assegnazioniDipendenti = InitAssegnazioni();

        private static Dictionary<string, List<int>> InitAssegnazioni()
        {
            // Pre-popola assegnazioni di esempio per le navi esistenti
            return new Dictionary<string, List<int>>
            {
                // Nave 1 - oggi, fascia mattina e pomeriggio (gruisti e mulettisti)
                { "1_0", new List<int> { 1, 7, 10, 2, 5 } },      // Mattina: gruisti 1,7,10 + mulettisti 2,5
                { "1_1", new List<int> { 14, 17, 20, 9, 15 } },   // Pomeriggio: gruisti 14,17,20 + mulettisti 9,15
                // Nave 2 - oggi, fascia sera (solo mulettisti)
                { "2_2", new List<int> { 2, 5, 9, 15, 18 } },     // Sera: mulettisti
                // Nave 3 - domani, tutte le fasce (solo mulettisti)
                { "3_0", new List<int> { 2, 5, 9, 15, 18 } },     // Mattina
                { "3_1", new List<int> { 22, 25, 12, 2, 5 } },    // Pomeriggio (alcuni diversi)
                { "3_2", new List<int> { 9, 15, 18, 22, 25 } }    // Sera
            };
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

            // Popola dati Pianificazione Turni
            CaricaDatiPianificazione(model);

            return View(model);
        }

        private void CaricaDatiPianificazione(IndexViewModel model)
        {
            var oggi = DateTime.Today;
            var domani = DateTime.Today.AddDays(1);
            var random = new Random();

            // Converti dipendenti da Gestione Dipendenti a DipendenteViewModel
            model.TuttiDipendenti = _dipendenti.Select(d => new DipendenteViewModel
            {
                Id = d.Id,
                Nome = d.Nome.Split(' ').Length > 1 ? d.Nome.Split(' ')[1] : d.Nome,
                Cognome = d.Nome.Split(' ')[0],
                Ruolo = d.Ruolo,
                PatenteScaduta = d.Patente && d.Scadenza.HasValue && d.Scadenza.Value < DateTime.Today,
                RichiedeVariazione = random.Next(100) < 15 // 15% probabilità variazione
            }).ToList();

            // Filtra gruisti e mulettisti disponibili
            model.Gruisti = model.TuttiDipendenti.Where(d => d.Ruolo == "Gruista").ToList();
            model.Mulettisti = model.TuttiDipendenti.Where(d => d.Ruolo == "Mulettista").ToList();

            // Navi oggi da Gestione Navi
            var naviOggiDb = _navi.Where(n => n.DataArrivo.Date == oggi).ToList();
            model.NaviOggi = naviOggiDb.Select(n => CreaNaveViewModel(n, model.TuttiDipendenti, random)).ToList();

            // Navi domani da Gestione Navi
            var naviDomaniDb = _navi.Where(n => n.DataArrivo.Date == domani).ToList();
            model.NaviDomani = naviDomaniDb.Select(n => CreaNaveViewModel(n, model.TuttiDipendenti, random)).ToList();

            model.OraCorrente = DateTime.Now.Hour;

            // Passa le assegnazioni per filtrare i dipendenti disponibili
            // Convertiamo le chiavi per includere il giorno (oggi=0, domani=1)
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

        private NaveViewModel CreaNaveViewModel(NaveDetailViewModel naveDb, List<DipendenteViewModel> tuttiDipendenti, Random random)
        {
            var nave = new NaveViewModel
            {
                Id = naveDb.Id,
                Nome = naveDb.Nome,
                Pontile = naveDb.Pontile ?? 0,
                FasciaMattina = naveDb.FasciaMattina,
                FasciaPomeriggio = naveDb.FasciaPomeriggio,
                FasciaSera = naveDb.FasciaSera,
                RichiedeGruisti = naveDb.RichiedeGruisti,
                RichiedeMulettisti = naveDb.RichiedeMulettisti
            };

            // Carica dipendenti salvati o lascia vuoto
            if (naveDb.FasciaMattina)
                nave.DipendentiMattina = GetDipendentiAssegnati(naveDb.Id, 0, tuttiDipendenti);
            if (naveDb.FasciaPomeriggio)
                nave.DipendentiPomeriggio = GetDipendentiAssegnati(naveDb.Id, 1, tuttiDipendenti);
            if (naveDb.FasciaSera)
                nave.DipendentiSera = GetDipendentiAssegnati(naveDb.Id, 2, tuttiDipendenti);

            return nave;
        }

        private List<DipendenteViewModel> GetDipendentiAssegnati(int naveId, int fascia, List<DipendenteViewModel> tuttiDipendenti)
        {
            var chiave = $"{naveId}_{fascia}";
            if (_assegnazioniDipendenti.TryGetValue(chiave, out var idDipendenti))
            {
                return tuttiDipendenti.Where(d => idDipendenti.Contains(d.Id)).ToList();
            }
            return new List<DipendenteViewModel>();
        }

        [HttpPost]
        public virtual IActionResult SalvaAssegnazione(int naveId, int fascia, [FromBody] List<int> dipendentiIds)
        {
            var chiave = $"{naveId}_{fascia}";
            _assegnazioniDipendenti[chiave] = dipendentiIds ?? new List<int>();
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
                }
            }
            return Json(new { success = true });
        }

        [HttpGet]
        public virtual IActionResult New()
        {
            return RedirectToAction(Actions.Edit());
        }

        [HttpGet]
        public virtual async Task<IActionResult> Edit(Guid? id)
        {
            var model = new EditViewModel();

            if (id.HasValue)
            {
                model.SetUser(await _sharedService.Query(new UserDetailQuery
                {
                    Id = id.Value,
                }));
            }



            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Edit(EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.Id = await _sharedService.Handle(model.ToAddOrUpdateUserCommand());

                    Alerts.AddSuccess(this, "Informazioni aggiornate");

                    // Esempio lancio di un evento SignalR
                    await _publisher.Publish(new NewMessageEvent
                    {
                        IdGroup = model.Id.Value,
                        IdUser = model.Id.Value,
                        IdMessage = Guid.NewGuid()
                    });
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.Message);
                }
            }

            if (ModelState.IsValid == false)
            {
                Alerts.AddError(this, "Errore in aggiornamento");
            }

            return RedirectToAction(Actions.Edit(model.Id));
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(Guid id)
        {
            // Query to delete user

            Alerts.AddSuccess(this, "Utente cancellato");

            return RedirectToAction(Actions.Index());
        }

        [HttpGet]
        public virtual IActionResult GestioneNavi(string giorno = null)
        {
            var oggi = DateTime.Today;
            var domani = DateTime.Today.AddDays(1);
            DateTime? giornoSelezionato = null;

            // Parse del giorno selezionato (se presente)
            if (!string.IsNullOrEmpty(giorno) && DateTime.TryParse(giorno, out var dataParsata))
            {
                // Solo se è un giorno futuro (da dopodomani in poi)
                if (dataParsata.Date > domani.Date)
                {
                    giornoSelezionato = dataParsata.Date;
                }
            }

            var model = new GestioneNaviViewModel
            {
                DataOggi = oggi,
                DataDomani = domani,
                GiornoSelezionato = giornoSelezionato,
                TutteLeNavi = _navi,
                NaviOggi = _navi.Where(n => n.DataArrivo.Date == oggi).ToList(),
                NaviDomani = _navi.Where(n => n.DataArrivo.Date == domani).ToList(),
                NaviGiornoSelezionato = giornoSelezionato.HasValue
                    ? _navi.Where(n => n.DataArrivo.Date == giornoSelezionato.Value).ToList()
                    : null
            };
            return View("Gestione_Navi", model);
        }

        [HttpGet]
        public virtual IActionResult DettaglioNave(int? id, string dataPreselezionata = null, bool nascondiCalendario = false)
        {
            NaveDetailViewModel nave;
            var oggi = DateTime.Today;
            var domani = DateTime.Today.AddDays(1);

            if (id.HasValue)
            {
                nave = _navi.FirstOrDefault(n => n.Id == id.Value);
                if (nave == null)
                {
                    return NotFound();
                }
                // Per navi esistenti di oggi o domani, nascondi il calendario
                if (nave.DataArrivo.Date == oggi || nave.DataArrivo.Date == domani)
                {
                    nascondiCalendario = true;
                }
            }
            else
            {
                // Nuova nave
                var dataArrivo = DateTime.Today;
                if (!string.IsNullOrEmpty(dataPreselezionata) && DateTime.TryParse(dataPreselezionata, out var dataParsata))
                {
                    dataArrivo = dataParsata;
                }

                nave = new NaveDetailViewModel
                {
                    Id = 0,
                    DataArrivo = dataArrivo,
                    Tipo = TipoNave.Container
                };
            }

            ViewBag.NascondiCalendario = nascondiCalendario;
            return PartialView("_DettaglioNave", nave);
        }

        [HttpPost]
        public virtual IActionResult SalvaNave(int id, string nome, int tipo, DateTime dataArrivo, int? pontile,
            bool fasciaMattina, bool fasciaPomeriggio, bool fasciaSera,
            bool richiedeGruisti, bool richiedeMulettisti)
        {
            // Validazione: almeno una fascia oraria deve essere selezionata
            if (!fasciaMattina && !fasciaPomeriggio && !fasciaSera)
            {
                Alerts.AddError(this, "Devi selezionare almeno una fascia oraria");
                return RedirectToAction(Actions.GestioneNavi());
            }

            // Validazione: verifica che le fasce selezionate non siano già occupate da altre navi
            var naviStessoGiorno = _navi.Where(n => n.DataArrivo.Date == dataArrivo.Date && n.Id != id).ToList();
            foreach (var altraNave in naviStessoGiorno)
            {
                if (fasciaMattina && altraNave.FasciaMattina)
                {
                    Alerts.AddError(this, $"La fascia 00:00-08:00 è già occupata dalla nave {altraNave.Nome}");
                    return RedirectToAction(Actions.GestioneNavi());
                }
                if (fasciaPomeriggio && altraNave.FasciaPomeriggio)
                {
                    Alerts.AddError(this, $"La fascia 08:00-16:00 è già occupata dalla nave {altraNave.Nome}");
                    return RedirectToAction(Actions.GestioneNavi());
                }
                if (fasciaSera && altraNave.FasciaSera)
                {
                    Alerts.AddError(this, $"La fascia 16:00-24:00 è già occupata dalla nave {altraNave.Nome}");
                    return RedirectToAction(Actions.GestioneNavi());
                }
            }

            if (id == 0)
            {
                // Nuova nave
                var nuovaNave = new NaveDetailViewModel
                {
                    Id = _nextNaveId++,
                    Nome = nome,
                    Tipo = (TipoNave)tipo,
                    DataArrivo = dataArrivo,
                    Pontile = pontile,
                    FasciaMattina = fasciaMattina,
                    FasciaPomeriggio = fasciaPomeriggio,
                    FasciaSera = fasciaSera,
                    RichiedeGruisti = richiedeGruisti,
                    RichiedeMulettisti = richiedeMulettisti,
                    Colore = ColoriNavi.GetColore(_navi.Count)
                };
                _navi.Add(nuovaNave);
                Alerts.AddSuccess(this, "Nave aggiunta con successo");
            }
            else
            {
                // Modifica nave esistente
                var nave = _navi.FirstOrDefault(n => n.Id == id);
                if (nave != null)
                {
                    nave.Nome = nome;
                    nave.Tipo = (TipoNave)tipo;
                    nave.DataArrivo = dataArrivo;
                    nave.Pontile = pontile;
                    nave.FasciaMattina = fasciaMattina;
                    nave.FasciaPomeriggio = fasciaPomeriggio;
                    nave.FasciaSera = fasciaSera;
                    nave.RichiedeGruisti = richiedeGruisti;
                    nave.RichiedeMulettisti = richiedeMulettisti;
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
                Alerts.AddSuccess(this, "Nave eliminata con successo");
            }

            return RedirectToAction(Actions.GestioneNavi());
        }

        [HttpGet]
        public virtual IActionResult TabellaNaviGiorno(string giorno)
        {
            if (string.IsNullOrEmpty(giorno) || !DateTime.TryParse(giorno, out var dataParsata))
            {
                return BadRequest();
            }

            var navi = _navi.Where(n => n.DataArrivo.Date == dataParsata.Date).ToList();
            ViewBag.DataGiorno = dataParsata;
            return PartialView("_TabellaNaviGiorno", navi);
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

            // Solo per Gruista e Mulettista gestisco patente e scadenza
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

            Alerts.AddSuccess(this, "Dipendente aggiornato con successo");
            return RedirectToAction(Actions.GestioneDipendenti());
        }
    }
}
