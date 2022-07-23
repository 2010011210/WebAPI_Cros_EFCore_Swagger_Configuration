using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo.Model
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class CustomAttribute : Attribute
    {
        public CustomAttribute() 
        {
        
        }

        public CustomAttribute(string name, string address)
        {
            this.Name = name;
            this.Address = address;
        }

        public string Name { get; set; }

        public string Address { get; set; }

        public string Description { get; set; }

        public string GetName() 
        {
            return Name;
        }
    }
}
