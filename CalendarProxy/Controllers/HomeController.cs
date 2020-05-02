using System.Web.Mvc;

namespace CalendarProxy.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        ///     Displays the default view, which allows to construct proxified calendar URL based on user-provided options
        /// </summary>
        /// <returns></returns>
        public ViewResult Index()
        {
            return View();
        }
    }
}