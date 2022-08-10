using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiDemo.Model.AOP;

namespace WebApiDemo.Model.MiddleWare
{
    /// <summary>
    /// 中间件扩展类
    /// </summary>
    public static class MiddleExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public static void UseCustomeAPIFilter(this IApplicationBuilder app) 
        {
            Func<RequestDelegate, RequestDelegate> func = next =>
            {
                return new RequestDelegate(context =>
                {
                    if (context.Request.Path.Value.Contains("api"))
                    {
                        return Task.Run(()=> 
                        {
                            context.Response.WriteAsync("This is api check, start \n");
                            context.Response.WriteAsync("This is api check, end \n");
                        });
                    }
                    else
                    {
                        return Task.Run(() =>
                        {
                            next.Invoke(context);
                        });
                    }
                });
            };

            app.Use(func);
        }

        public static IApplicationBuilder UseStaticPage(this IApplicationBuilder app,string directoryPath, bool isDelete) 
        {
            return app.UseMiddleware<StaticPagaMiddleware>("D:\\WangCong\\data", true);
        }
    }
}
