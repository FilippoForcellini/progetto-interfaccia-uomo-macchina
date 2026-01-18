using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pianificazioneturni.Web.Infrastructure;
using PianificazioneTurni.Services.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Pianificazioneturni.Web.Areas.Example.Users
{
    public class IndexViewModel : PagingViewModel
    {
        public IndexViewModel()
        {
            OrderBy = nameof(UserIndexViewModel.Email);
            OrderByDescending = false;
            Users = Array.Empty<UserIndexViewModel>();
            NaviOggi = new List<NaveViewModel>();
            NaviDomani = new List<NaveViewModel>();
            TuttiDipendenti = new List<DipendenteViewModel>();
        }

        [Display(Name = "Cerca")]
        public string Filter { get; set; }

        public IEnumerable<UserIndexViewModel> Users { get; set; }

        // Pianificazione Turni
        public DateTime DataOggi { get; set; } = DateTime.Today;
        public DateTime DataDomani { get; set; } = DateTime.Today.AddDays(1);
        public int OraCorrente { get; set; } = DateTime.Now.Hour;
        public List<NaveViewModel> NaviOggi { get; set; }
        public List<NaveViewModel> NaviDomani { get; set; }
        public List<DipendenteViewModel> TuttiDipendenti { get; set; }
        public List<DipendenteViewModel> Gruisti { get; set; } = new List<DipendenteViewModel>();
        public List<DipendenteViewModel> Mulettisti { get; set; } = new List<DipendenteViewModel>();

        // Assegnazioni per filtrare dipendenti disponibili (chiave: "naveId_fascia_giorno", valore: lista id dipendenti)
        public Dictionary<string, List<int>> Assegnazioni { get; set; } = new Dictionary<string, List<int>>();

        internal void SetUsers(UsersIndexDTO usersIndexDTO)
        {
            Users = usersIndexDTO.Users.Select(x => new UserIndexViewModel(x)).ToArray();
            TotalItems = usersIndexDTO.Count;
        }

        public UsersIndexQuery ToUsersIndexQuery()
        {
            return new UsersIndexQuery
            {
                Filter = Filter,
                Paging = new PianificazioneTurni.Infrastructure.Paging
                {
                    OrderBy = OrderBy,
                    OrderByDescending = OrderByDescending,
                    Page = Page,
                    PageSize = PageSize
                }
            };
        }

        public override IActionResult GetRoute() => MVC.Example.Users.Index(this).GetAwaiter().GetResult();

        public string OrderbyUrl<TProperty>(IUrlHelper url, System.Linq.Expressions.Expression<Func<UserIndexViewModel, TProperty>> expression) => base.OrderbyUrl(url, expression);

        public string OrderbyCss<TProperty>(HttpContext context, System.Linq.Expressions.Expression<Func<UserIndexViewModel, TProperty>> expression) => base.OrderbyCss(context, expression);

        public string ToJson()
        {
            return JsonSerializer.ToJsonCamelCase(this);
        }
    }

    public class UserIndexViewModel
    {
        public UserIndexViewModel(UsersIndexDTO.User userIndexDTO)
        {
            this.Id = userIndexDTO.Id;
            this.Email = userIndexDTO.Email;
            this.FirstName = userIndexDTO.FirstName;
            this.LastName = userIndexDTO.LastName;
        }

        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    // Classi per Pianificazione Turni
    public enum StatoNave { InLavorazione, InPartenza, InArrivo }
    public enum FasciaOraria { Mattina = 0, Pomeriggio = 1, Sera = 2 }

    public class NaveViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int Pontile { get; set; }
        public bool FasciaMattina { get; set; }
        public bool FasciaPomeriggio { get; set; }
        public bool FasciaSera { get; set; }
        public bool RichiedeGruisti { get; set; }
        public bool RichiedeMulettisti { get; set; }

        // Dipendenti assegnati per fascia
        public List<DipendenteViewModel> DipendentiMattina { get; set; } = new List<DipendenteViewModel>();
        public List<DipendenteViewModel> DipendentiPomeriggio { get; set; } = new List<DipendenteViewModel>();
        public List<DipendenteViewModel> DipendentiSera { get; set; } = new List<DipendenteViewModel>();

        // Calcola lo stato in base all'orario corrente
        public StatoNave GetStato(int oraCorrente)
        {
            // Determina in quale fascia siamo
            // Mattina: 0-8, Pomeriggio: 8-16, Sera: 16-24
            bool inFasciaMattina = oraCorrente >= 0 && oraCorrente < 8;
            bool inFasciaPomeriggio = oraCorrente >= 8 && oraCorrente < 16;
            bool inFasciaSera = oraCorrente >= 16 && oraCorrente < 24;

            // Verifica se la nave è attiva nella fascia corrente
            bool naveAttivaOra = (inFasciaMattina && FasciaMattina) ||
                                 (inFasciaPomeriggio && FasciaPomeriggio) ||
                                 (inFasciaSera && FasciaSera);

            if (naveAttivaOra)
                return StatoNave.InLavorazione;

            // Verifica se la nave arriverà più tardi oggi
            if (inFasciaMattina && (FasciaPomeriggio || FasciaSera))
                return StatoNave.InArrivo;
            if (inFasciaPomeriggio && FasciaSera)
                return StatoNave.InArrivo;

            // Altrimenti la nave è in partenza (già passata)
            return StatoNave.InPartenza;
        }

        public string GetStatoDescrizione(int oraCorrente)
        {
            return GetStato(oraCorrente) switch
            {
                StatoNave.InLavorazione => "In lavorazione",
                StatoNave.InPartenza => "In Partenza",
                StatoNave.InArrivo => "In arrivo",
                _ => ""
            };
        }

        // Verifica se una fascia specifica è "in arrivo" (futura)
        public bool IsFasciaInArrivo(FasciaOraria fascia, int oraCorrente)
        {
            int inizioFascia = fascia switch
            {
                FasciaOraria.Mattina => 0,
                FasciaOraria.Pomeriggio => 8,
                FasciaOraria.Sera => 16,
                _ => 0
            };
            return oraCorrente < inizioFascia;
        }
    }

    public class DipendenteViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Ruolo { get; set; }
        public bool PatenteScaduta { get; set; }
        public bool RichiedeVariazione { get; set; }

        public string NomeCompleto => $"{Cognome} {Nome}";
    }
}
