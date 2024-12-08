using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webCore.MongoHelper
{
    public class AuthenticateHelper: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionToken = context.HttpContext.Session.GetString("AdminToken");

            if (sessionToken == null)
            {
                context.Result = new RedirectToActionResult("Index", "Admin_singin", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
