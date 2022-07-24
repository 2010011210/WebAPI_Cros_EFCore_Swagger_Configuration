using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonCore
{
    public class CustomerMonitorInterceptor : IInterceptor
    {
        /// <summary>
        /// 拦截--invocation.Proceed() 等同于原方法的调用
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;
            var argument = invocation.Arguments;

            Console.WriteLine($"method:{method}--argument:{argument}");

            invocation.Proceed();
        }

    }
}
