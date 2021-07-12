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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApiDemo.Model;

namespace WebApiDemo
{
  public class Startup
  {
    public static ILoggerRepository LogRepository { get; set; }
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
      LogRepository = LogManager.CreateRepository("NETCoreRepository");   //仓库的名字可以在配置文件中配置，也可以直接写死
      XmlConfigurator.Configure(LogRepository, new FileInfo("log4Config.xml"));  //读取配置文件

    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();
      //read configuration file
      //services.AddOptions();
      //services.Configure<Logging>(Configuration.GetSection("logging1"));

      //cors
      services.AddCors(options => options.AddPolicy("AllowAll",policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

      //swagger UI
      services.AddSwaggerGen(c => {
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
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      //cors
      app.UseCors("AllowAll");
      //swagger
      app.UseSwagger();
      app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json","v1testTittle");
      });

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
