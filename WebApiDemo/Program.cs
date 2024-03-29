using Autofac;
using Autofac.Extras.DynamicProxy;
using Autofac.Extensions.DependencyInjection;
using IterfaceTest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonCore;

namespace WebApiDemo
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())  //�滻IOC����
            .ConfigureContainer<ContainerBuilder>((context, conainerBuilder)=>   //ע��
            {
                conainerBuilder.RegisterType<ServiceA>().As<ITestServiceA>().EnableInterfaceInterceptors();
                conainerBuilder.RegisterType<CustomerMonitorInterceptor>();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder.UseStartup<Startup>();
            });
  }
}
