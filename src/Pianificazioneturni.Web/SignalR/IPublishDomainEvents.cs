using System.Threading.Tasks;

namespace Pianificazioneturni.Web.SignalR
{
    public interface IPublishDomainEvents
    {
        Task Publish(object evnt);
    }
}
