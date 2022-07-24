using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo.Model
{
    public class LogActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string url = context.HttpContext.Request.Path.Value;
            string argument = JsonConvert.SerializeObject(context.ActionArguments);

            string controllerName = context.Controller.GetType().FullName;
            string actionName = context.ActionDescriptor.DisplayName;

            Console.WriteLine($"url--{url},---argument:{argument}");

        }
    }
}
