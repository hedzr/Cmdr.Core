using System;

namespace HzNS.Cmdr.Tool.Ext
{
    public static class ConvertibleExtensions
    {
        public static T? ConvertToNullable<T>(this IConvertible convertibleValue) where T : struct
        {
            if (null == convertibleValue)
            {
                return null;
            }

            return (T?) Convert.ChangeType(convertibleValue, typeof(T));
        }

        public static T ConvertTo<T>(this IConvertible convertibleValue)
        {
            if (null == convertibleValue)
            {
                return default(T);
            }

            if (!typeof(T).IsGenericType)
            {
                return (T) Convert.ChangeType(convertibleValue, typeof(T));
            }
            else
            {
                var genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    return (T) Convert.ChangeType(convertibleValue, Nullable.GetUnderlyingType(typeof(T)));
                }
            }

            throw new InvalidCastException(
                $"Invalid cast from type \"{convertibleValue.GetType().FullName}\" to type \"{typeof(T).FullName}\".");
        }
    }
}