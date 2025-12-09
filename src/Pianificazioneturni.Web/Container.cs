using Microsoft.Extensions.DependencyInjection;
using Pianificazioneturni.Web.SignalR;
using PianificazioneTurni.Services.Shared;

namespace Pianificazioneturni.Web
{
    public class Container
    {
        public static void RegisterTypes(IServiceCollection container)
        {
            // Registration of all the database services you have
            container.AddScoped<SharedService>();

            // Registration of SignalR events prova
            container.AddScoped<IPublishDomainEvents, SignalrPublishDomainEvents>();
        }
    }
}
