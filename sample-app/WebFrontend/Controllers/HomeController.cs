using System.Web.Mvc;
using Cafe.Web.ActionFilters;

namespace Cafe.Web.Controllers
{
    [IncludeLayoutData]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
