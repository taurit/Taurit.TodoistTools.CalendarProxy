using System.Web.Mvc;

namespace CalendarProxy.Controllers
{
    public class ErrorController : Controller
    {
        /// <summary>
        ///     Returns generic error page
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            Response.StatusCode = 400;
            return View();
        }
    }
}