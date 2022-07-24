using Castle.DynamicProxy;
using IterfaceTest;
using Microsoft.AspNetCore.Mvc;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiDemo.Model;

namespace WebApiDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[LogActionFilterAttribute]
    public class AOPController : ControllerBase
    {
        [HttpGet]
        [Route("Get")]
        public string Get(int i) 
        {
            if ( i == 500) 
            {
                throw new Exception("AOP Index Exception");
            }

            return $"{typeof(AOPController)}_Get";
        }

        [HttpGet]
        [Route("GetCastleDynamicProxy")]
        public string GetCastleDynamicProxy(int i)
        {
            if (i == 500)
            {
                throw new Exception("AOP Index Exception");
            }

            ProxyGenerator generator = new ProxyGenerator();
            CustomInterceptor interceptor = new CustomInterceptor();
            ITestServiceA serviceA = new ServiceA();

            var iTestServiceA = generator.CreateInterfaceProxyWithTarget<ITestServiceA>(serviceA, interceptor);

            iTestServiceA.Show();

            return $"{typeof(AOPController)}_Get";
        }
    }
}
