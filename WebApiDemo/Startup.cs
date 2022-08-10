using IterfaceTest;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApiDemo.Model;
using WebApiDemo.Model.AOP;
using WebApiDemo.Model.MiddleWare;

namespace WebApiDemo
{
    public class Startup
    {
        public static ILoggerRepository LogRepository { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            LogRepository = LogManager.CreateRepository("NETCoreRepository");   //�ֿ�����ֿ����������ļ������ã�Ҳ����ֱ��д��
            XmlConfigurator.Configure(LogRepository, new FileInfo("log4Config.xml"));  //��ȡ�����ļ�

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddControllersWithViews(options => {
                options.Filters.Add<LogActionFilterAttribute>();
                options.Filters.Add<CustomExceptionFilterAttribute>();
            }).AddControllersAsServices();

            //read configuration file
            //services.AddOptions();
            //services.Configure<Logging>(Configuration.GetSection("logging1"));

            //cors
            services.AddCors(options => options.AddPolicy("AllowAll", policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

            //swagger UI
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "ThisIsTitle", Version = "ThisIsVersion" });
            });

            #region  EF Coding first
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<TestContext>(options =>
            options.UseSqlServer(connectionString));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            #endregion


            #region IOC

            //services.AddTransient<ITestServiceA, ServiceA>();


            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region �Զ����м��
            //Func<RequestDelegate, RequestDelegate> middleware = new Func<RequestDelegate, RequestDelegate>(
            //    next =>
            //    {
            //        return new RequestDelegate(context => 
            //            { 
            //                return Task.Run(() =>
            //                    { 
            //                        context.Response.WriteAsync("This is Tom middleWare test begin \n");
            //                        next.Invoke(context);
            //                        context.Response.WriteAsync("This is Tom middleWare test end \n");
            //                    }
            //                ); 
            //            }
            //        );
            //    }
            //);

            //Func<RequestDelegate, RequestDelegate> middleware2 = next => async context =>
            //{
            //    await context.Response.WriteAsync("This is Tom middleWare2 test begin \n");
            //    await next.Invoke(context);
            //    await context.Response.WriteAsync("This is Tom middleWare2 test end \n");
            //};

            //app.Use(middleware);
            //app.Use(middleware2);
            //app.Use(next => async context =>
            //{
            //    await context.Response.WriteAsync("This is Tom middleWare3 test begin \n");
            //    await next.Invoke(context);
            //    await context.Response.WriteAsync("This is Tom middleWare3 test end \n");
            //});
            //app.Use(next => async context =>
            //{
            //    await context.Response.WriteAsync("This is Tom middleWare4 test begin \n");
            //    //await next.Invoke(context);    //���һ��������м������Ҫ���ô����ί�е�invoke��������Ϊ���һ����Ĭ�ϵ�ί�У��ᱨ��
            //    await context.Response.WriteAsync("This is Tom middleWare4 test end \n");
            //});

            app.UseCustomeAPIFilter();

            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //cors
            app.UseCors("AllowAll");
            //swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1testTittle");
            });

            app.UseRouting();
            //StaticPagaMiddleware
            //app.UseMiddleware<StaticPagaMiddleware>();
            //app.UseMiddleware<StaticPagaMiddleware>("D:\\WangCong\\data", true);
            app.UseStaticPage("D:\\WangCong\\data", true);

            app.UseMiddleware<HeaderReadWriteMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }




    }
}
