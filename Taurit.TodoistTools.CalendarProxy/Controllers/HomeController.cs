using Microsoft.AspNetCore.Mvc;

namespace Taurit.TodoistTools.CalendarProxy.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        ///     Displays the default view, which allows to construct proxified calendar URL based on user-provided options
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }
    }
}