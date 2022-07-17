using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApiDemo.Model
{
    /// <summary>
    /// 委托扩展，针对linq. 可以把User类换成泛型T
    /// </summary>
    public static class DelegateExtend
    {
        public static List<User> TomWhere(this List<User> lists,Func<User, bool> func) 
        {
            List<User> users = new List<User>();
            foreach (var user in lists) 
            {
                Thread.Sleep(2000);
                Console.WriteLine($"Before:userid:{user.id},username:{user.firstName}");
                if (func.Invoke(user)) 
                {
                    users.Add(user);
                    Console.WriteLine($"After:userid:{user.id},username:{user.firstName}");
                }
            }

            return users;
        }

        /// <summary>
        /// 迭代器返回
        /// </summary>
        /// <param name="lists"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IEnumerable<User> TomEnumeatorWhere(this List<User> lists, Func<User, bool> func)
        {
            List<User> users = new List<User>();
            foreach (var user in lists)
            {
                Thread.Sleep(2000);
                Console.WriteLine($"Before:userid:{user.id},username:{user.firstName}");
                if (func.Invoke(user))
                {
                    yield return user;
                    Console.WriteLine($"After:userid:{user.id},username:{user.firstName}");
                }
            }
        }

    }
}
