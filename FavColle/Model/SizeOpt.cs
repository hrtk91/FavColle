using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FavColle.Model
{
    public class EnumNameAttribute : Attribute
    {
        public string Value { get; protected set; }

        public EnumNameAttribute(string value)
        {
            Value = value;
        }
    }
    public static class CommonAttribute
    {
        public static string Attribute(this Enum @enum)
        {
            Type type = @enum.GetType();
            var fieldInfo = type.GetField(@enum.ToString());

            if (fieldInfo == null) return "";

            return (fieldInfo.GetCustomAttributes(typeof(EnumNameAttribute), false).First() as EnumNameAttribute)?.Value;
        }
    }

    public enum SizeOpt
    {
        [EnumName("thumb")]
        Thumb,
        [EnumName("small")]
        Small,
        [EnumName("medium")]
        Medium,
        [EnumName("large")]
        Large,
        [EnumName("orig")]
        Orig
    }
}
