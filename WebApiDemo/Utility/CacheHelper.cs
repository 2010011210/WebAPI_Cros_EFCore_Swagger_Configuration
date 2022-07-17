using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApiDemo.Utility
{
    /// <summary>
    /// 缓存类
    /// </summary>
    public class CacheHelper
    {
        
        static CacheHelper() 
        {
            Task.Run(() => 
                {
                    while (true) 
                    {
                        List<string> keyList = new List<string>();
                        foreach (var key in shopCache.Keys)
                        {
                            if (shopCache[key].Key < DateTime.Now)
                            {
                                keyList.Add(key);
                            }
                        }

                        keyList.ForEach(key => shopCache.Remove(key, out KeyValuePair<DateTime, object> val));
                        Thread.Sleep(1000 * 60 * 2);
                    };
                }
            );
        }

        /// <summary>
        /// 缓存的字典
        /// </summary>
        //private static Dictionary<string, KeyValuePair<DateTime,object>> shopCache = new Dictionary<string, KeyValuePair<DateTime, object>>();
        private static ConcurrentDictionary<string, KeyValuePair<DateTime, object>> shopCache = new ConcurrentDictionary<string, KeyValuePair<DateTime, object>>();

        private static Dictionary<string, object> LockObject = new Dictionary<string, object>();

        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Exist(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (shopCache.ContainsKey(key))
            {
                if (shopCache[key].Key < DateTime.Now)
                {
                    shopCache.TryRemove(key, out KeyValuePair<DateTime, object> res);
                    return false;
                }
                else 
                {
                    return true;
                }
            }
            else 
            {
                return false;
            }
        }

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="seconds">过期时间</param>
        public static void Set(string key, object val, int seconds = 30)  
        {
            if (string.IsNullOrEmpty(key)) 
            {
                return;
            }

            if (seconds == 30) 
            {
                seconds += new Random().Next(1,10);
            }
            //lock (LockObject)  //Diction多线程可能会又问题，要加锁。或者用ConcurrentDiction,这个是线程安全的。
            //{
            //    shopCache[key] = val;
            //}

            shopCache[key] = new KeyValuePair<DateTime, object>(DateTime.Now.AddSeconds(seconds), val);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key) 
        {
            return (T)(shopCache[key].Value);
        }

        /// <summary>
        /// 把获取数据的方法按照委托传进来
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="fun"></param>
        /// <param name="seconds">过期时间</param>
        /// <returns></returns>
        public static T Get<T>(string key, Func<T> fun, int seconds = 30)
        {
            T t = default(T);
            if (Exist(key))
            {
                t = Get<T>(key);
            }
            else 
            {
                t = fun.Invoke();
                Set(key, t, seconds);
            }
            return t;
        }



        /// <summary>
        /// 删除某个缓存
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key) 
        {
            if (Exist(key)) 
            {
                shopCache.TryRemove(key, out KeyValuePair<DateTime, object> res);
            }
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public static void RemoveAll()
        {
            shopCache.Clear();
        }

        /// <summary>
        /// 条件删除缓存
        /// </summary>
        /// <param name="func">委托，判断是否删除的方法</param>
        public static void RemoveCondition(Func<string, bool> func) 
        {
            List<string> keyList = new List<string>();
            foreach (var key in shopCache.Keys) 
            {
                if (func.Invoke(key)) 
                {
                    keyList.Add(key);
                }
            }

            keyList.ForEach(key => shopCache.TryRemove(key, out KeyValuePair<DateTime, object> res));
        }


    }
}
