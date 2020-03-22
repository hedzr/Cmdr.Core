using System.Diagnostics.Contracts;
using System.Text;

namespace HzNS.Cmdr.Tool.Ext
{
    public static class StringExtensions
    {
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
        public static string Repeat(this System.String @this, char ch, int repeatCount)
        {
            return new string(ch, repeatCount);
        }

        // public static IOrderedEnumerable<TSource> OrderBy2<TSource, TKey>(
        //     this IEnumerable<TSource> source,
        //     Func<TSource, TKey> keySelector)
        // {
        //     return null;// (IOrderedEnumerable<TSource>) new OrderedEnumerable<TSource, TKey>(source, keySelector, (IComparer<TKey>) null, false, (OrderedEnumerable<TSource>) null);
        // }
    }
}