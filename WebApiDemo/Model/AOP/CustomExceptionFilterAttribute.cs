using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo.Model
{
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public new int Order = 50;

        public override void OnException(ExceptionContext context)
        {
            Console.WriteLine($"This is {nameof(CustomExceptionFilterAttribute)} OnException {this.Order}");

            if (!context.ExceptionHandled) 
            {
                Console.WriteLine($"This {nameof(CustomExceptionFilterAttribute)}  OnException {this.Order}  --Inside");
            }

            context.Result = new JsonResult(new {
                Result = false,
                Message = "context.Exception.Message"
            });
            context.ExceptionHandled = true;
        }

    }
}
