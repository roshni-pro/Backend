using System.Web.Mvc;

namespace AngularJSAuthentication.API.Controllers.MVC
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View("~/MvcViews/Home/Index.cshtml");
        }
    }
}