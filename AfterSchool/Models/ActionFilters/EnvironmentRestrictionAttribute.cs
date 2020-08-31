using System.Web.Mvc;

namespace AfterSchool.Models.ActionFilters
{
    public class EnvironmentRestrictionAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!AfterSchoolLibrary.Helpers.Utilities.IsLocalHost && !AfterSchoolLibrary.Helpers.Utilities.IsProduction)
                filterContext.Result = new RedirectResult("/");
        }
    }
}