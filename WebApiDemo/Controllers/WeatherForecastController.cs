using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiDemo.Model;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using log4net;

namespace WebApiDemo.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class WeatherForecastController : ControllerBase
  {
    //type 是指你当前使用log的对象的类，例如需要在HomeController中使用，参数就为typeof(HomeController)
    private readonly ILog _log = LogManager.GetLogger(Startup.LogRepository.Name, typeof(WeatherForecastController));
    private static readonly string[] Summaries = new[]
    {
      "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private IConfiguration _configuration;
    public Logging logging { get; set; }


    public WeatherForecastController(IConfiguration configuration)
    {
      _configuration = configuration;
      //logging = options.Value;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
      var rng = new Random();
      return Enumerable.Range(1, 5).Select(index => new WeatherForecast
      {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = rng.Next(-20, 55),
        Summary = Summaries[rng.Next(Summaries.Length)]
      })
      .ToArray();
    }

    [HttpPost]
    public string Post(object user) {
      //var config = _configuration.GetSection("logging1").Get<Logging>();
      //User userModel = JsonConvert.DeserializeObject<User>(user.ToString());
      //var request = HttpContext.Request;
      //_log.Info(userModel.firstName);
      //_log.Error(DateTime.Now.ToString() + " error test");
      try {
        using (var content = new TestContext())
        {
          var log = logging;
          var a = content.cars;
          var name = Guid.NewGuid().ToString();
          content.cars.Add(new Car() { carName = name ,createTime = DateTime.Now });
          content.SaveChanges();
        }
      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
      }
      return $"post success:{200}";
    }
  }
}
