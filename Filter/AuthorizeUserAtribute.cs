using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace shopflowerproject.Filters
{
    public class AuthorizeUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userRole = context.HttpContext.Session.GetString("role");

            if (!string.Equals(userRole, "User", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("Denied", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
