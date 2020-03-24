using System;
using System.Data;
using System.Reflection;

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
#pragma warning disable CS8653
                return default; // default(T)
#pragma warning restore CS8653
            }

            if (!typeof(T).IsGenericType)
            {
                return (T) Convert.ChangeType(convertibleValue, typeof(T));
            }
            else
            {
                var genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
#pragma warning disable CS8604
                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    return (T) Convert.ChangeType(convertibleValue, Nullable.GetUnderlyingType(typeof(T)));
                }
#pragma warning restore CS8604
            }

            throw new InvalidCastException(
                $"Invalid cast from type \"{convertibleValue.GetType().FullName}\" to type \"{typeof(T).FullName}\".");
        }


        //


        // ReSharper disable once InconsistentNaming
        private static readonly Type _nullableType = typeof(Nullable<>);


        // ReSharper disable once MemberCanBePrivate.Global
        public static void SetValue<T>(object value, PropertyInfo property, T model)
        {
            if (!property.CanWrite)
                return;

            var isNullable = property.PropertyType.IsGenericType &&
                             property.PropertyType.GetGenericTypeDefinition() == _nullableType;
            var targetType = property.PropertyType;

            if (isNullable)
                targetType = property.PropertyType.GetGenericArguments()[0];

            // ReSharper disable once InvertIf
            if (targetType.IsValueType || targetType.Name == "String") //避免自定义Class等非基础类型抛出异常
            {
#pragma warning disable CS8604
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (targetType.IsEnum)
                    property.SetValue(model, Enum.Parse(targetType, value.ToString()), null);
                else
                    property.SetValue(model, Convert.ChangeType(value, targetType), null);
#pragma warning restore CS8604
            }
        }

        public static void SetValue<T>(DataRow dr, PropertyInfo property, string colName, T model)
        {
            if (colName == null)
                colName = property.Name;
            if (dr.Table.Columns.Contains(colName) && dr[colName].GetType().Name != "DBNull")
            {
                SetValue(dr[property.Name], property, model);
            }
        }


        //
    }
}