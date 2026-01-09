using Microsoft.AspNetCore.Mvc;

namespace Pianificazioneturni.Web.Features.Home
{
    public partial class HomeController : Controller
    {
        public virtual IActionResult Index()
        {
            return View();
        }

        public virtual IActionResult GestioneNavi()
        {
            return View();
        }
    }
}
