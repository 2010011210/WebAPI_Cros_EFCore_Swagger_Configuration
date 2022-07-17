using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo.Utility
{
    /// <summary>
    /// 数据提供类
    /// </summary>
    public class DataSource
    {
        /// <summary>
        /// 模拟查询数据库
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static int QueryDb(int i) 
        {
            int res = 0;
            for (var k = 0; k < 5; k++) 
            {
                res += i;
            }
            return res;
        }

        /// <summary>
        /// 模拟查询数据库
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static int QueryFromRides(int i)
        {
            System.Diagnostics.Debug.Write($"QueryFromRides({i})");
            Console.WriteLine($"ConsoleWrite:QueryFromRides({i})");
            int res = 1;
            for (var k = 0; k < 5; k++)
            {
                res *= i;
            }
            Console.WriteLine($"QueryFromRides({i}),result：{res}");
            return res;
        }
    }
}
