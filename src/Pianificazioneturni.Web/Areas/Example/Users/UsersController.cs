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
                new DipendenteDetailViewModel { Id = 1, Nome = "Rossi Mario", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2026, 1, 1) },
                new DipendenteDetailViewModel { Id = 2, Nome = "Blu Marco", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2025, 12, 2) },
                new DipendenteDetailViewModel { Id = 3, Nome = "Bianchi Filippo", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2025, 6, 15) },
                new DipendenteDetailViewModel { Id = 4, Nome = "Cortesi Giulia", Ruolo = "Addetto terminal", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 5, Nome = "Gialli Monica", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2026, 3, 20) },
                new DipendenteDetailViewModel { Id = 6, Nome = "Verdi Luca", Ruolo = "Ormeggiatore", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 7, Nome = "Azzurri Margherita", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2025, 2, 10) },
                new DipendenteDetailViewModel { Id = 8, Nome = "Viola Riccardo", Ruolo = "Addetto alla Sicurezza", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 9, Nome = "Arancioni Sofia", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2025, 11, 30) },
                new DipendenteDetailViewModel { Id = 10, Nome = "Celeste Lorenzo", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2026, 8, 25) },
                new DipendenteDetailViewModel { Id = 11, Nome = "Rosa Alex", Ruolo = "Addetto terminal", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 12, Nome = "Neri Federico", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2025, 1, 5) },
                new DipendenteDetailViewModel { Id = 13, Nome = "Marroni Eleonora", Ruolo = "Ormeggiatore", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 14, Nome = "Grigi Roberto", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2025, 9, 18) },
                new DipendenteDetailViewModel { Id = 15, Nome = "Lavanda Francesco", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2026, 5, 12) },
                new DipendenteDetailViewModel { Id = 16, Nome = "Giannini Matteo", Ruolo = "Addetto alla Sicurezza", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 17, Nome = "Forcellini Filippo", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2025, 4, 22) },
                new DipendenteDetailViewModel { Id = 18, Nome = "Limoni Marta", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2026, 2, 14) },
                new DipendenteDetailViewModel { Id = 19, Nome = "Acqua Filomena", Ruolo = "Addetto terminal", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 20, Nome = "Fuochi Davide", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2025, 8, 8) },
                new DipendenteDetailViewModel { Id = 21, Nome = "Lampone Federica", Ruolo = "Ormeggiatore", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 22, Nome = "Agnelli Lucia", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2025, 3, 30) },
                new DipendenteDetailViewModel { Id = 23, Nome = "Rinaldi Martina", Ruolo = "Addetto alla Sicurezza", Patente = false, Scadenza = null },
                new DipendenteDetailViewModel { Id = 24, Nome = "Tonelli Alessandro", Ruolo = "Gruista", Patente = true, Scadenza = new DateTime(2026, 7, 19) },
                new DipendenteDetailViewModel { Id = 25, Nome = "Nardelli Tommaso", Ruolo = "Mulettista", Patente = true, Scadenza = new DateTime(2025, 10, 25) }
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
            // Lista dipendenti
            model.TuttiDipendenti = new List<DipendenteViewModel>
            {
                new DipendenteViewModel { Id = 1, Nome = "Mario", Cognome = "Rossi" },
                new DipendenteViewModel { Id = 2, Nome = "Luigi", Cognome = "Verdi" },
                new DipendenteViewModel { Id = 3, Nome = "Lucia", Cognome = "Gialli" },
                new DipendenteViewModel { Id = 4, Nome = "Marco", Cognome = "Blu", PatenteScaduta = true },
                new DipendenteViewModel { Id = 5, Nome = "Sofia", Cognome = "Bianchi", RichiedeVariazione = true },
                new DipendenteViewModel { Id = 6, Nome = "Paolo", Cognome = "Neri" },
                new DipendenteViewModel { Id = 7, Nome = "Anna", Cognome = "Rosa" },
                new DipendenteViewModel { Id = 8, Nome = "Marco", Cognome = "Grigi" },
                new DipendenteViewModel { Id = 9, Nome = "Sara", Cognome = "Viola" },
                new DipendenteViewModel { Id = 10, Nome = "Luca", Cognome = "Aranci" },
                new DipendenteViewModel { Id = 11, Nome = "Claudio", Cognome = "Neri" },
                new DipendenteViewModel { Id = 12, Nome = "Luca", Cognome = "Azzurri", PatenteScaduta = true },
                new DipendenteViewModel { Id = 13, Nome = "Lucia", Cognome = "Rosa" },
            };

            // Navi oggi
            model.NaviOggi = new List<NaveViewModel>
            {
                new NaveViewModel
                {
                    Id = 1,
                    Nome = "Nave 1",
                    Stato = StatoNave.InLavorazione,
                    Pontile = 10,
                    Fascia = FasciaOraria.Mattina,
                    Dipendenti = new List<DipendenteViewModel>
                    {
                        model.TuttiDipendenti[0],
                        model.TuttiDipendenti[1],
                        model.TuttiDipendenti[2],
                        model.TuttiDipendenti[3],
                        model.TuttiDipendenti[4]
                    }
                },
                new NaveViewModel
                {
                    Id = 2,
                    Nome = "Nave 2",
                    Stato = StatoNave.InPartenza,
                    Pontile = 30,
                    Fascia = FasciaOraria.Pomeriggio,
                    Dipendenti = new List<DipendenteViewModel>
                    {
                        model.TuttiDipendenti[5],
                        model.TuttiDipendenti[6],
                        model.TuttiDipendenti[7],
                        model.TuttiDipendenti[8],
                        model.TuttiDipendenti[9]
                    }
                }
            };

            // Navi domani
            model.NaviDomani = new List<NaveViewModel>
            {
                new NaveViewModel
                {
                    Id = 3,
                    Nome = "Nave 3",
                    Stato = StatoNave.InArrivo,
                    Pontile = 20,
                    Fascia = FasciaOraria.Sera,
                    Dipendenti = new List<DipendenteViewModel>
                    {
                        model.TuttiDipendenti[0],
                        model.TuttiDipendenti[10],
                        model.TuttiDipendenti[11],
                        model.TuttiDipendenti[12],
                        new DipendenteViewModel { Id = 2, Nome = "Luigi", Cognome = "Verdi", RichiedeVariazione = true }
                    }
                }
            };
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
        public virtual IActionResult DettaglioNave(int? id, string dataPreselezionata = null)
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
