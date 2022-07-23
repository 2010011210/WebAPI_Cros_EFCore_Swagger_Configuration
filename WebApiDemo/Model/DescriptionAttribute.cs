using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo.Model
{
    public class ColumnNameAttribute : Attribute
    {
        public ColumnNameAttribute()
        {
        }

        public ColumnNameAttribute(string des)
        {
            this.Description = des;
        }

        public ColumnNameAttribute(string des, string name)
        {
            this.Description = des;
            this.Name = name;
        }


        public string Description { get; set; }
        public string Name { get; set; }

        public string GetDescription() 
        {
            return Description;
        }

        public string GetName()
        {
            return Name;
        }
    }
}
