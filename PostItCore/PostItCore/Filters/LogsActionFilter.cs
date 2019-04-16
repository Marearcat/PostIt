using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.Filters
{
    public class LogsActionFilter : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Response.Cookies.Append("LastCheckLogs", DateTime.Now.ToString("dd/MM/yyyy hh-mm-ss"));
        }
    }
}
