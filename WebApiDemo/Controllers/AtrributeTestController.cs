using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WebApiDemo.Model;

namespace WebApiDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AtrributeTestController : ControllerBase
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet]
        [Route("GetName")]
        public string GetName(int i)
        {
            string res = "";
            var car = new Car("car") { };

            Type type = typeof(Car);
            if (type.IsDefined(typeof(CustomAttribute), true)) 
            {
                var attributeList = type.GetCustomAttributes(typeof(CustomAttribute),true);
                foreach (CustomAttribute item in attributeList) 
                {
                    var name = item.Name;
                    res = name;
                }

                var properties = type.GetProperties();

                foreach (PropertyInfo item in properties) 
                {
                    res += item.GetName() + "||";
                    //if (item.IsDefined(typeof(ColumnNameAttribute),false)) 
                    //{
                    //    var attributeList2 = item.GetCustomAttributes(typeof(ColumnNameAttribute), true);
                    //    foreach (ColumnNameAttribute colum in attributeList2)
                    //    {
                    //        var columnName = colum.GetDescription();
                    //        res += columnName + "/n";
                    //    }
                    //}
                }

            }

            return res;
        }
    }
}
