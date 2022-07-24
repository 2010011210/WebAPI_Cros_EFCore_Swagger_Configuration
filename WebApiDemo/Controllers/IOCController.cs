using IterfaceTest;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IOCController : ControllerBase
    {
        private readonly ITestServiceA _iTestServiceA;
        public IOCController(ITestServiceA testServiceA) 
        {
            this._iTestServiceA = testServiceA;
        }

        public string Index() 
        {
            this._iTestServiceA.Show();
            return "Ok";
        }

    }
}
