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
        public List<NaveViewModel> NaviOggi { get; set; }
        public List<NaveViewModel> NaviDomani { get; set; }
        public List<DipendenteViewModel> TuttiDipendenti { get; set; }

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
    public enum FasciaOraria { Mattina, Pomeriggio, Sera }

    public class NaveViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public StatoNave Stato { get; set; }
        public int Pontile { get; set; }
        public FasciaOraria Fascia { get; set; }
        public List<DipendenteViewModel> Dipendenti { get; set; } = new List<DipendenteViewModel>();

        public string StatoDescrizione => Stato switch
        {
            StatoNave.InLavorazione => "In lavorazione",
            StatoNave.InPartenza => "In Partenza",
            StatoNave.InArrivo => "In arrivo",
            _ => ""
        };

        public string StatoIcona => Stato switch
        {
            StatoNave.InLavorazione => "in_lavorazione.png",
            StatoNave.InPartenza => "in_partenza.png",
            StatoNave.InArrivo => "in_arrivo.png",
            _ => ""
        };

        public string FasciaDescrizione => Fascia switch
        {
            FasciaOraria.Mattina => "00:00 - 08:00",
            FasciaOraria.Pomeriggio => "08:00 - 16:00",
            FasciaOraria.Sera => "16:00 - 24:00",
            _ => ""
        };
    }

    public class DipendenteViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public bool PatenteScaduta { get; set; }
        public bool RichiedeVariazione { get; set; }

        public string NomeCompleto => $"{Cognome} {Nome}";
    }
}
