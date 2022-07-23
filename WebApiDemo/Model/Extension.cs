using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WebApiDemo.Model
{
    public static class Extension
    {
        public static string GetName(this MemberInfo info) 
        {
            if (info.IsDefined(typeof(CustomAttribute), false))
            {
                CustomAttribute attribute = info.GetCustomAttribute<CustomAttribute>();
                return attribute.GetName();
            } 
            else if(info.IsDefined(typeof(ColumnNameAttribute), false))
            {
                ColumnNameAttribute attribute = info.GetCustomAttribute<ColumnNameAttribute>();
                return attribute.GetName();

            }
            return info.Name;
        
        }

        public static string GetDescription(this MemberInfo info)
        {
           if (info.IsDefined(typeof(ColumnNameAttribute), false))
            {
                ColumnNameAttribute attribute = info.GetCustomAttribute<ColumnNameAttribute>();
                return attribute.GetDescription();

            }
            return info.Name;

        }
    }
}
