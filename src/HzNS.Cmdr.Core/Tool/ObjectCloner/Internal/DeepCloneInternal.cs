#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

#if DEBUG

#endif

namespace HzNS.Cmdr.Tool.ObjectCloner.Internal
{
    internal delegate object DeepCloner(object original, Dictionary<object, object> dict);

    internal static class DeepCloneInternal
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ConcurrentDictionary<Type, DeepCloner> _clonerMap =
            new ConcurrentDictionary<Type, DeepCloner>();

        /// <summary>
        ///     Function for the internal implementation of deep cloning.
        ///     First argument is the object to be cloned, second is a map in which already-cloned objects are stored. Call with an empty Dictionary initially.
        /// </summary>
        /// <remarks>
        ///     The <see cref="Dictionary{TKey,TValue}"/> argument is used for handling circular references and multiple references to the same object.
        /// </remarks>
        public static DeepCloner GetDeepCloner(Type type)
        {
            return _clonerMap.GetOrAdd(type, t =>
            {
                if (TypeHelper.CanSkipDeepClone(type))
                    return Identity;

                var builder = new DeepCloneExpressionBuilder(t);
                Expression<DeepCloner> finalExpression = builder.Build();

#if DEBUG
                // Debug.WriteLine("################");
                // Debug.WriteLine(type.Name);
                // Debug.WriteLine("################");
                // Debug.WriteLine(finalExpression.ToReadableString()); // need: AgileObjects.ReadableExpressions
#endif

                return finalExpression.Compile();
            });
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object Identity(object input, Dictionary<object, object> _) => input;
    }
}