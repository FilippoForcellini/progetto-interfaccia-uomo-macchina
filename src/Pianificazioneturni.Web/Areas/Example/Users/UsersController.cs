using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Pianificazioneturni.Web.Infrastructure;
using Pianificazioneturni.Web.SignalR;
using Pianificazioneturni.Web.SignalR.Hubs.Events;
using PianificazioneTurni.Services.Shared;
using PianificazioneTurni.Services.Pianificazione;
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
        private readonly PianificazioneDbContext _context; //_context -> istanza di Entity Framework che connette il codice al database SQL Server.

        private NaveDetailViewModel MapToNaveViewModel(Nave nave)
        {
            var vm = new NaveDetailViewModel
            {
                Id = nave.Id,
                Nome = nave.Nome,
                Tipo = (TipoNave)nave.Tipo,
                Pontile = nave.Pontile,
                DataArrivo = nave.DataArrivo,
                OrarioArrivo = nave.OrarioArrivo,
                DataPartenza = nave.DataPartenza,
                OrarioPartenza = nave.OrarioPartenza,
                RichiedeGruisti = nave.RichiedeGruisti,
                RichiedeMulettisti = nave.RichiedeMulettisti,
                RichiedeAddettiTerminal = nave.RichiedeAddettiTerminal,
                RichiedeOrmeggiatori = nave.RichiedeOrmeggiatori,
                RichiedeAddettiSicurezza = nave.RichiedeAddettiSicurezza
            };
            vm.CalcolaDateEFasce();
            return vm;
        }

        private DipendenteDetailViewModel MapToDipendenteViewModel(Dipendente dipendente)
        {
            return new DipendenteDetailViewModel
            {
                Id = dipendente.Id,
                Nome = dipendente.Nome,
                Ruolo = dipendente.Ruolo,
                Patente = dipendente.Patente,
                Scadenza = dipendente.Scadenza
            };
        }

        public UsersController(SharedService sharedService, IPublishDomainEvents publisher, IStringLocalizer<SharedResource> sharedLocalizer, PianificazioneDbContext context)
        {
            _sharedService = sharedService;
            _publisher = publisher;
            _sharedLocalizer = sharedLocalizer;
            _context = context; //usato per tutte le operazioni su navi/dipendenti

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

            //carica dipendenti da DbContext
            var dipendentiDb = _context.Dipendenti.ToList();
            model.TuttiDipendenti = dipendentiDb.Select(d => new DipendenteViewModel
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

            //carica le navi da DbContext
            var tutteLeNavi = _context.Navi.ToList();
            var tutteLeNaviVm = tutteLeNavi.Select(n => MapToNaveViewModel(n)).ToList();

            //Navi oggi da Gestione Navi
            var naviOggiDb = tutteLeNaviVm.Where(n => n.DatePresenza.Any(d => d.Date == oggi)).ToList();
            model.NaviOggi = naviOggiDb.Select(n => CreaNaveViewModel(n, model.TuttiDipendenti, random, oggi)).ToList();

            //Navi domani da Gestione Navi
            var naviDomaniDb = tutteLeNaviVm.Where(n => n.DatePresenza.Any(d => d.Date == domani)).ToList();
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
            var assegnazioni = _context.Assegnazioni
                .Where(a => a.NaveId == naveId && a.Fascia == fascia)
                .OrderBy(a => a.Id)
                .ToList();

            return assegnazioni
                .Select(a => tuttiDipendenti.FirstOrDefault(d => d.Id == a.DipendenteId))
                .Where(d => d != null)
                .ToList();
        }

        [HttpPost]
        public virtual IActionResult SalvaAssegnazione(int naveId, int fascia, [FromBody] List<int> dipendentiIds)
        {
            var existing = _context.Assegnazioni
                .Where(a => a.NaveId == naveId && a.Fascia == fascia)
                .ToList();
            _context.Assegnazioni.RemoveRange(existing);

            if (dipendentiIds != null && dipendentiIds.Any())
            {
                foreach (var dipId in dipendentiIds)
                {
                    _context.Assegnazioni.Add(new Assegnazione
                    {
                        NaveId = naveId,
                        DipendenteId = dipId,
                        Fascia = fascia
                    });
                }
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public virtual IActionResult CambiaDipendente(int naveId, int fascia, int vecchioDipendenteId, int nuovoDipendenteId)
        {
            var assegnazione = _context.Assegnazioni
                .FirstOrDefault(a => a.NaveId == naveId && a.Fascia == fascia && a.DipendenteId == vecchioDipendenteId);

            if (assegnazione != null)
            {
                assegnazione.DipendenteId = nuovoDipendenteId;
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public virtual IActionResult EliminaDipendente(int naveId, int fascia, int dipendenteId)
        {
            var assegnazione = _context.Assegnazioni
                .FirstOrDefault(a => a.NaveId == naveId && a.Fascia == fascia && a.DipendenteId == dipendenteId);

            if (assegnazione != null)
            {
                _context.Assegnazioni.Remove(assegnazione);
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public virtual IActionResult EliminaTurno(int naveId, int fascia)
        {
            var assegnazioni = _context.Assegnazioni
                .Where(a => a.NaveId == naveId && a.Fascia == fascia)
                .ToList();

            if (assegnazioni.Any())
            {
                _context.Assegnazioni.RemoveRange(assegnazioni);
                _context.SaveChanges();
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

            var tutteLeNavi = _context.Navi.ToList().Select(n => MapToNaveViewModel(n)).ToList();

            var model = new GestioneNaviViewModel
            {
                DataOggi = oggi,
                DataDomani = domani,
                GiornoSelezionato = giornoSelezionato,
                TutteLeNavi = tutteLeNavi,
                NaviOggi = tutteLeNavi.Where(n => n.DatePresenza.Any(d => d.Date == oggi)).ToList(),
                NaviDomani = tutteLeNavi.Where(n => n.DatePresenza.Any(d => d.Date == domani)).ToList(),
                NaviGiornoSelezionato = giornoSelezionato.HasValue
                    ? tutteLeNavi.Where(n => n.DatePresenza.Any(d => d.Date == giornoSelezionato.Value)).ToList()
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
                var naveDb = _context.Navi.FirstOrDefault(n => n.Id == id.Value);
                if (naveDb == null)
                {
                    return NotFound();
                }
                nave = MapToNaveViewModel(naveDb);
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
                var nuovaNave = new Nave
                {
                    Nome = nome,
                    Tipo = (int)TipoNave.Container,
                    Pontile = pontile,
                    DataArrivo = parsedDataArrivo,
                    OrarioArrivo = orarioArrivo,
                    DataPartenza = parsedDataPartenza,
                    OrarioPartenza = orarioPartenza,
                    RichiedeGruisti = richiedeGruisti,
                    RichiedeMulettisti = richiedeMulettisti,
                    RichiedeAddettiTerminal = richiedeAddettiTerminal,
                    RichiedeOrmeggiatori = richiedeOrmeggiatori,
                    RichiedeAddettiSicurezza = richiedeAddettiSicurezza
                };
                _context.Navi.Add(nuovaNave);
                _context.SaveChanges();
                Alerts.AddSuccess(this, "Nave aggiunta con successo");
            }
            else
            {
                //Modifica nave esistente
                var nave = _context.Navi.FirstOrDefault(n => n.Id == id);
                if (nave != null)
                {
                    nave.Nome = nome;
                    nave.Pontile = pontile;
                    nave.DataArrivo = parsedDataArrivo;
                    nave.OrarioArrivo = orarioArrivo;
                    nave.DataPartenza = parsedDataPartenza;
                    nave.OrarioPartenza = orarioPartenza;
                    nave.RichiedeGruisti = richiedeGruisti;
                    nave.RichiedeMulettisti = richiedeMulettisti;
                    nave.RichiedeAddettiTerminal = richiedeAddettiTerminal;
                    nave.RichiedeOrmeggiatori = richiedeOrmeggiatori;
                    nave.RichiedeAddettiSicurezza = richiedeAddettiSicurezza;
                    _context.SaveChanges();
                    Alerts.AddSuccess(this, "Nave aggiornata con successo");
                }
            }

            return RedirectToAction(Actions.GestioneNavi());
        }

        [HttpPost]
        public virtual IActionResult EliminaNave(int id)
        {
            var nave = _context.Navi.FirstOrDefault(n => n.Id == id);
            if (nave != null)
            {
                _context.Navi.Remove(nave);
                _context.SaveChanges();
                Alerts.AddSuccess(this, "Nave eliminata con successo");
            }

            return RedirectToAction(Actions.GestioneNavi());
        }


        [HttpGet]
        public virtual IActionResult GestioneDipendenti(bool filtroPatentiScadute = false)
        {
            var dipendenti = _context.Dipendenti.ToList()
                .Select(d => MapToDipendenteViewModel(d))
                .ToList();

            var model = new GestioneDipendentiViewModel
            {
                Dipendenti = dipendenti,
                FiltroPatentiScadute = filtroPatentiScadute
            };

            return View("Gestione_Dipendenti", model);
        }

        [HttpGet]
        public virtual IActionResult DettaglioDipendente(int id)
        {
            var dipendenteDb = _context.Dipendenti.FirstOrDefault(d => d.Id == id);
            if (dipendenteDb == null)
            {
                return NotFound();
            }

            var dipendente = MapToDipendenteViewModel(dipendenteDb);
            ViewBag.Ruoli = RuoliDipendente.GetRuoli();
            return PartialView("_DettaglioDipendente", dipendente);
        }

        [HttpPost]
        public virtual IActionResult SalvaDipendente(DipendenteDetailViewModel model)
        {
            var dipendente = _context.Dipendenti.FirstOrDefault(d => d.Id == model.Id);
            if (dipendente == null)
            {
                return NotFound();
            }

            dipendente.Ruolo = model.Ruolo;

            //determina se il ruolo richiede la patente
            var richiedePatente = model.Ruolo == "Gruista" || model.Ruolo == "Mulettista";

            if (richiedePatente)
            {
                dipendente.Patente = model.Patente;
                dipendente.Scadenza = model.Scadenza;
            }
            else
            {
                dipendente.Patente = false;
                dipendente.Scadenza = null;
            }

            _context.SaveChanges();
            Alerts.AddSuccess(this, "Dipendente aggiornato con successo");
            return RedirectToAction(Actions.GestioneDipendenti());
        }
    }
}
