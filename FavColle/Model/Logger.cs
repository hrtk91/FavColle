using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FavColle.Model.Interface;

namespace FavColle.Model
{
    class Logger
    {
        private Logger() { }

        private class LoggerImpl : ILogger
        {
            public void Print(string message, Exception e)
            {
                Console.WriteLine("{0} Error={0}:{1}", message, e.GetType(), e.Message);

                using (var sw = new System.IO.StreamWriter("./error.log"))
                {
                    sw.WriteAsync(string.Format("\r\n{0}:{1}\r\n{2}rn", e.GetType(), e.Message, e.StackTrace));
                }
            }
        }

        private static ILogger SingleLogger { get; } = new LoggerImpl();

        public static ILogger GetLogger() => SingleLogger;
    }
}
