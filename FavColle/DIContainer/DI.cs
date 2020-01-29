using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FavColle.DIContainer
{
    public class DI
    {
        protected static IDictionary<string, object> objects = new Dictionary<string, object>();

        public static T Register<T>() where T : class, new()
        {
            var obj = new T();
            objects.Add(typeof(T).Name, obj);

            return obj;
        }


        public static T Get<T>() where T : class, new()
        {
            return (T)objects.Where(pair => pair.Key == typeof(T).Name).First().Value;
        }
    }
}
