using IterfaceTest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service
{
    public class ServiceA : ITestServiceA
    {
        public ServiceA() 
        {
            Console.WriteLine("ServiceA 被构建");
        }

        public void Show() 
        {
            Console.WriteLine($"this is ServiceA.Show");
        }

        public void Show(int i, string name)
        {
            Console.WriteLine($"this is ServiceA.Show i:{i}__name:{name}");
        }


    }
}
