using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Pianificazioneturni.Web.Infrastructure;
using Pianificazioneturni.Web.SignalR;
using Pianificazioneturni.Web.SignalR.Hubs.Events;
using PianificazioneTurni.Services.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pianificazioneturni.Web.Areas.Example.Users
{
    [Area("Example")]
    public partial class UsersController : AuthenticatedBaseController
    {
        private readonly SharedService _sharedService;
        private readonly IPublishDomainEvents _publisher;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

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
        public virtual IActionResult GestioneNavi()
        {
            return View("Gestione_Navi");
        }

        [HttpGet]
        public virtual IActionResult GestioneDipendenti()
        {
            return View("Gestione_Dipendenti");
        }
    }
}
