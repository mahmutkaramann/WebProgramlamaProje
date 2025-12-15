using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace YeniSalon.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var memberInfo = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault();

            if (memberInfo != null)
            {
                var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    return displayAttribute.Name ?? enumValue.ToString();
                }
            }

            return enumValue.ToString();
        }
    }
}