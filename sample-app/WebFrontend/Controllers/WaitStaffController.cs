using System.Web.Mvc;
using Cafe.Web.ActionFilters;

namespace Cafe.Web.Controllers
{
    [IncludeLayoutData]
    public class WaitStaffController : Controller
    {
        public ActionResult Todo(string id)
        {
            ViewBag.Waiter = id;

            return View(Domain.OpenTabQueries.TodoListForWaiter(id));
        }
    }
}
