using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo
{
  public class ConfigurationManager
  {
    public readonly static IConfiguration Configuration;

    static ConfigurationManager()
    {
      Configuration = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
         .Build();
    }

    //public static T GetSection<T>(string key) where T : class, new()
    //{
    //  return new Microsoft.Extensions.DependencyInjection.ServiceCollection()
    //      .AddOptions()
    //      .Configure<T>(Configuration.GetSection(key))
    //      .BuildServiceProvider()
    //      .GetService<IOptions<T>>()
    //      .Value;
    //}

    public static string GetSection(string key)
    {
      return Configuration.GetValue<string>(key);
    }
  }
}
