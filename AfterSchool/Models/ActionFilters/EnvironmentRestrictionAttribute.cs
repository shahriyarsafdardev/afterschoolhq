using System.Web.Mvc;

namespace AfterSchool.Models.ActionFilters
{
    public class EnvironmentRestrictionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!AfterSchoolLibrary.Helpers.Utilities.IsLocalHost && !AfterSchoolLibrary.Helpers.Utilities.IsProduction)
                filterContext.Result = new RedirectResult("/");
        }
    }
}