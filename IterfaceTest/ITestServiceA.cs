using Autofac.Extras.DynamicProxy;
using CommonCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IterfaceTest
{
    [Intercept(typeof(CustomerMonitorInterceptor))]
    public interface ITestServiceA
    {
        void Show();

        void Show(int i, string name);

    }
}
