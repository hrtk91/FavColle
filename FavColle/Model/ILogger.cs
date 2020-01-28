using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FavColle.Model
{
    interface ILogger
    {
        void Print(string message, Exception e);
    }
}
