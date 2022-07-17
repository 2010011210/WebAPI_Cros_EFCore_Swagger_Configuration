 using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApiDemo.Model;

namespace WebApiDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeletgateTestController : ControllerBase
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet]
        [Route("Tom")]
        public IEnumerable<User> GetTom() 
        {
            List<User> users = new List<User>() { };
            users.Add(new User() { id=1,firstName ="Tom"});
            users.Add(new User() { id=2,firstName ="Jack"});
            users.Add(new User() { id=3,firstName ="Eric"});
            var list = users.TomWhere(s => s.id > 1);

            foreach (var user in list) 
            {
                Console.WriteLine($"{user.id}:{user.firstName}");
            }

            return users.ToArray();
        }

        [HttpGet]
        [Route("Get")]
        public IEnumerable<User> Get()
        {
            List<User> users = new List<User>() { };
            users.Add(new User() { id = 1, firstName = "Tom" });
            users.Add(new User() { id = 2, firstName = "Jack" });
            users.Add(new User() { id = 3, firstName = "Eric" });
            var list = users.Where(s => {
                Thread.Sleep(2000);
                return s.id > 1;
            });

            foreach (var user in list)
            {
                Console.WriteLine($"{user.id}:{user.firstName}");
            }

            return users.ToArray();
        }

        [HttpGet]
        [Route("GetV2")]
        public IEnumerable<User> GetV2()
        {
            List<User> users = new List<User>() { };
            users.Add(new User() { id = 1, firstName = "Tom" });
            users.Add(new User() { id = 2, firstName = "Jack" });
            users.Add(new User() { id = 3, firstName = "Eric" });
            var list = users.TomEnumeatorWhere(s => {
                return s.id > 1;
            });

            foreach (var user in list)
            {
                Console.WriteLine($"{user.id}:{user.firstName}");
            }

            return users.ToArray();
        }

    }
}
