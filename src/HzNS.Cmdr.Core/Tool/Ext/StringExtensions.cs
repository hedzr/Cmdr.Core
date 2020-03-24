using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HzNS.Cmdr.Tool.Ext
{
    public static class StringEx
    {
        [Pure]
        // [PublicAPI]
        // ReSharper disable once BuiltInTypeReferenceStyle
        public static string Repeat(char ch, int repeatCount)
        {
            return new string(ch, repeatCount);
        }
    }


    public static class StringExtensions
    {
        public static string EatStart(this string @this, string part)
        {
            return @this.StartsWith(part) ? @this.Substring(part.Length) : @this;
        }

        public static string EatEnd(this string @this, string part)
        {
            return @this.EndsWith(part) ? @this.Substring(0, @this.Length - part.Length) : @this;
        }

        public static string EatBoth(this string @this, string part)
        {
            var t = @this;
            if (t.StartsWith(part))
                t = t.Substring(part.Length);
            if (t.EndsWith(part))
                t = t.Substring(0, t.Length - part.Length);
            return t;
        }


        /// <summary>
        ///     A string extension method that repeats the string a specified number of times.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="repeatCount">Number of repeats.</param>
        /// <returns>The repeated string.</returns>
        public static string Repeat(this string @this, int repeatCount)
        {
            if (@this.Length == 1)
            {
                return new string(@this[0], repeatCount);
            }

            // int[] ints = { 10, 45, 15, 39, 21, 26 };
            // var result = ints.OrderBy2(g => g);

            var sb = new StringBuilder(repeatCount * @this.Length);
            // while (repeatCount-- > 0)
            // {
            //     sb.Append(@this);
            // }
            sb.Insert(0, @this, repeatCount);
            return sb.ToString();
        }

        [Pure]
        // [PublicAPI]
        // ReSharper disable once BuiltInTypeReferenceStyle
        public static string Repeat(this String @this, char ch, int repeatCount)
        {
            return new string(ch, repeatCount);
        }

        // public static IOrderedEnumerable<TSource> OrderBy2<TSource, TKey>(
        //     this IEnumerable<TSource> source,
        //     Func<TSource, TKey> keySelector)
        // {
        //     return null;// (IOrderedEnumerable<TSource>) new OrderedEnumerable<TSource, TKey>(source, keySelector, (IComparer<TKey>) null, false, (OrderedEnumerable<TSource>) null);
        // }


        /// <summary>Extracts a substring between specified delimiter strings.</summary>
        /// <example><code><![CDATA[
        /// var theExtractedString = "Hallo (Welt)!".Extract(between: "(", and: ")"); // Welt
        /// ]]></code></example>
        /// <param name="value">String</param>
        /// <param name="between">Left delimiter</param>
        /// <param name="and">Right delimiter</param>
        /// <returns>Extracted string</returns>
        public static string Extract(this string value, string between, string and)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (between == null)
            {
                throw new ArgumentNullException(nameof(between));
            }

            if (and == null)
            {
                throw new ArgumentNullException(nameof(and));
            }

            int theLeftIndex = value.IndexOf(between, StringComparison.Ordinal);
            if (theLeftIndex == -1)
            {
                return string.Empty;
            }

            int theRightIndex = value.IndexOf(and, StringComparison.Ordinal);
            if (theRightIndex == -1)
            {
                return value.Substring(theLeftIndex + 1);
            }

            if (theRightIndex < theLeftIndex)
            {
                return string.Empty;
            }

            return value.Substring(theLeftIndex + between.Length, theRightIndex - theLeftIndex - and.Length);
        }

        /// <summary>Determines whether the specified string matches the specified
        /// <paramref name="regexPattern"/>.</summary>
        /// <param name="value">String</param>
        /// <param name="regexPattern">Regex pattern</param>
        /// <returns><b>true</b>, if the pattern matches the string; otherwise <b>false</b></returns>
        public static bool Matches(this string value, string regexPattern) => Regex.IsMatch(value, regexPattern);

        /// <summary>Determines whether the specified string is a valid email address.</summary>
        /// <param name="value">String</param>
        /// <returns><b>true</b>, if the specified string is a valid email address;
        /// otherwise <b>false</b></returns>
        public static bool IsValidEmailAddress(this string value) =>
            Regex.IsMatch(
                value,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase
            );

        /// <summary> Tries to convert a string into an instance of <paramref name="destinationType"/>.
        /// </summary>
        /// <remarks>The <see cref="System.Globalization.CultureInfo.CurrentCulture"/> is used for
        /// conversion.</remarks>
        /// <param name="value">String</param>
        /// <param name="destinationType">Destination type</param>
        /// <param name="convertedValue">Converted value result</param>
        /// <returns><b>true</b>, if the conversion succeeds; otherwise <b>false</b></returns>
        public static bool TryConvert(this string value, Type destinationType, out object? convertedValue) =>
            TryConvert(value, destinationType, CultureInfo.CurrentCulture, out convertedValue);

        /// <summary> Tries to convert a string into an instance of <paramref name="destinationType"/>
        /// with the specified <paramref name="cultureInfo"/>.</summary>
        /// <param name="value">String</param>
        /// <param name="destinationType">Destination type</param>
        /// <param name="cultureInfo">Culture info</param>
        /// <param name="convertedValue">Converted value result</param>
        /// <returns><b>true</b>, if the conversion succeeds; otherwise <b>false</b></returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static bool TryConvert(this string value, Type destinationType, CultureInfo cultureInfo,
            out object? convertedValue)
        {
            try
            {
                var isNullableType = destinationType.IsGenericType &&
                                     destinationType.GetGenericTypeDefinition() == typeof(Nullable<>);
                var theDestinationType =
                    isNullableType ? destinationType.GetGenericArguments().Single() : destinationType;

                var theConverter = TypeDescriptor.GetConverter(theDestinationType);
                if (!theConverter.CanConvertFrom(typeof(string)))
                {
                    convertedValue = null;
                    return false;
                }

                var theValue = theConverter.ConvertFromString(null, cultureInfo, value);
                convertedValue = isNullableType ? Activator.CreateInstance(destinationType, theValue) : theValue;

                return true;
            }
            catch (System.Exception)
            {
                convertedValue = null;
                return false;
            }
        }

        /// <summary> Tries to convert a string into an instance of <typeparamref name="TDestinationType"/>.
        /// </summary>
        /// <remarks>The <see cref="System.Globalization.CultureInfo.CurrentCulture"/> is used for
        /// conversion.</remarks>
        /// <typeparam name="TDestinationType">Destination type</typeparam>
        /// <param name="value">String</param>
        /// <param name="convertedValue">Converted value result</param>
        /// <returns><b>true</b>, if the conversion succeeds; otherwise <b>false</b></returns>
        public static bool TryConvert<TDestinationType>(this string value, out TDestinationType convertedValue) =>
            TryConvert(value, CultureInfo.CurrentCulture, out convertedValue);

        /// <summary> Tries to convert a string into an instance of <typeparamref name="TDestinationType"/>.
        /// with the specified <paramref name="cultureInfo"/>.</summary>
        /// <typeparam name="TDestinationType">Destination type</typeparam>
        /// <param name="value">String</param>
        /// <param name="cultureInfo">Culture info</param>
        /// <param name="convertedValue">Converted value result</param>
        /// <returns><b>true</b>, if the conversion succeeds; otherwise <b>false</b></returns>
        public static bool TryConvert<TDestinationType>(this string value, CultureInfo cultureInfo,
            out TDestinationType convertedValue)
        {
            var theResult = TryConvert(value, typeof(TDestinationType), cultureInfo, out var theConvertedValue);

            if (theResult)
            {
                convertedValue = (TDestinationType) theConvertedValue!;
            }
            else
            {
                convertedValue = default!;
            }

            return theResult;
        }
    }
}