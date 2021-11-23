using DbManipulation.Enums;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DbManipulation
{
    public static class Helper
    {
        public static string GetDescription(this Enum @enum)
        {
            var fi = @enum.GetType().GetField(@enum.ToString());


            if (fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Any())
            {
                return attributes.First().Description;
            }

            return @enum.ToString();
        }

        public static StatusEnum GetEnumValue(short status)
        {
            return status switch
            {
                0 => StatusEnum.Pending,
                1 => StatusEnum.InProgress,
                2 => StatusEnum.Error,
                3 => StatusEnum.Done,
                _ => throw new ArgumentException(),
            };
        }
    }
}
