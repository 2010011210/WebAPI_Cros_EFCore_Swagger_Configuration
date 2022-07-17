using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiDemo.Utility;

namespace WebApiDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CacheTestController : ControllerBase
    {
        //public IActionResult Index()
        //{
        //    return ViewResult("");
        //}

        /// <summary>
        /// 获取年龄
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        [HttpGet]
        public int GetAge(int i) 
        {
            //var key = $"CEO_ID_{i}";
            //if (CacheHelper.Exist(key)) 
            //{
            //    return CacheHelper.Get<int>(key);
            //}
            //var ires = DataSource.QueryDb(i);
            //CacheHelper.Set(key, res);

            int res = -1;

            {
                var keyRedis = $"CEO_ID_Redis_{i}";
                var iRes = CacheHelper.Get<int>(keyRedis, () => DataSource.QueryFromRides(i));
            }

            {
                var keyDb = $"CEO_ID_Db_{i}";
                res = CacheHelper.Get<int>(keyDb, () => DataSource.QueryDb(i));
            }



            return res;
        }


    }
}
