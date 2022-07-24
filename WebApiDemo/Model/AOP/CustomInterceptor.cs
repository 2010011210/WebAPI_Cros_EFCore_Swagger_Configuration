using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo.Model
{
    public class CustomInterceptor : StandardInterceptor
    {
        protected override void PreProceed(IInvocation invocation)
        {
            Console.WriteLine($"当前调用方法：{invocation.Method.Name}");
        }

        protected override void PerformProceed(IInvocation invocation)
        {
            invocation.Proceed();
        }

        protected override void PostProceed(IInvocation invocation)
        {
            Console.WriteLine($"已经调用方法：{invocation.Method.Name}");
        }
    }
}
