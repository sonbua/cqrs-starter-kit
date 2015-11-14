using System.Web.Mvc;

namespace Cafe.Web.ActionFilters
{
    public class IncludeLayoutDataAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var viewResult = filterContext.Result as ViewResult;

            if (viewResult != null)
            {
                var bag = viewResult.ViewBag;
                bag.WaitStaff = StaticData.WaitStaff;
                bag.ActiveTables = Domain.OpenTabQueries.ActiveTableNumbers();
            }
        }
    }
}
