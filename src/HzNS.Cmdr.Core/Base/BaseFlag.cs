#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public abstract class BaseFlag<T> : BaseOpt, IFlag<T>, IEquatable<BaseFlag<T>>
    {
#pragma warning disable CS8618
        // ReSharper disable once PublicConstructorInAbstractClass
        public BaseFlag()
        {
        }
#pragma warning restore CS8618

        // ReSharper disable once PublicConstructorInAbstractClass
        public BaseFlag(string shortTitle, string longTitle, string[] aliases, string description,
            string descriptionLong, string examples, T defaultValue, string placeholder) : base(shortTitle, longTitle,
            aliases, description,
            descriptionLong, examples)
        {
            DefaultValue = defaultValue;
            PlaceHolder = placeholder;
        }

        public T DefaultValue { get; set; }

        public string PlaceHolder
        {
            get => _placeHolder;
            set => _placeHolder = value;
        }

        public string ToggleGroup
        {
            get => _toggleGroup;
            set
            {
                _toggleGroup = value;
                if (string.IsNullOrEmpty(Group))
                    Group = value;
            }
        }

        public bool UseMomentTimeFormat
        {
            get => _useMomentTimeFormat;
            set => _useMomentTimeFormat = value;
        }

        public int HitCount
        {
            get => _hitCount;
            set => _hitCount = value;
        }

        private string _toggleGroup = "";
        private int _hitCount = 0;
        private bool _useMomentTimeFormat = true;
        private string _placeHolder = "";


        public object? getDefaultValue()
        {
            return DefaultValue;
        }


        public BaseFlag<T> AddDefaultValue(T val)
        {
            DefaultValue = val;
            return this;
        }

        public bool Equals(BaseFlag<T>? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return // EqualityComparer<T>.Default.Equals(DefaultValue, other.DefaultValue) && 
                base.Equals(other) &&
                PlaceHolder == other.PlaceHolder && ToggleGroup == other.ToggleGroup &&
                UseMomentTimeFormat == other.UseMomentTimeFormat &&
                HitCount == other.HitCount;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseFlag<T>) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( // DefaultValue, 
                base.GetHashCode(),
                PlaceHolder, ToggleGroup, UseMomentTimeFormat, HitCount);
        }

        public static bool operator ==(BaseFlag<T>? left, BaseFlag<T>? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BaseFlag<T>? left, BaseFlag<T>? right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"Flag['{Long}', For:'{Owner?.backtraceTitles}']";
        }
    }
}