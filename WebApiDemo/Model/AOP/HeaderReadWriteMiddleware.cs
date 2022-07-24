using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo.Model.AOP
{
    public class HeaderReadWriteMiddleware
    {
        private readonly RequestDelegate _next;

        public HeaderReadWriteMiddleware(RequestDelegate next) 
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context) 
        {
            context.Response.OnStarting(async state => {
                var httpcontext = (HttpContext)state;
                httpcontext.Response.Headers.Add("middleware","Tom");
            }, context);

            context.Response.OnCompleted(async state => {
                var httpcontext = (HttpContext)state;
                Console.WriteLine($"请求结果：{httpcontext.Response.StatusCode}");
            }, context);

            await this._next.Invoke(context);
        }



    }
}
