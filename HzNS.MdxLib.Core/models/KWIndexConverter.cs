using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace HzNS.MdxLib.models
{
    #region KWIndexConverter

    public class KwIndexConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(KwIndex2))
                return true;
            if (destinationType == typeof(KwIndex1))
                return true;
            if (destinationType == typeof(KwIndex2List))
                return true;
            if (destinationType == typeof(KwIndexList))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
            object value, Type destinationType)
        {
            if (destinationType == typeof(String))
            {
                if (value is KwIndex2)
                {
                    KwIndex2 so = (KwIndex2) value;
                    return string.Format("{0}", so);
                }

                if (value is KwIndex1)
                {
                    KwIndex1 so = (KwIndex1) value;
                    return string.Format("{0}", so);
                }

                if (value is KwIndex2List)
                {
                    KwIndex2List so = (KwIndex2List) value;
                    return string.Format("{1} items", 0, so.Count);
                }

                if (value is KwIndexList)
                {
                    KwIndexList so = (KwIndexList) value;
                    return string.Format("{1} items", 0, so.Count);
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            bool b = base.CanConvertFrom(context, sourceType);
            return b;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture,
            object value)
        {
            if (value is string)
            {
                PropertyInfo pi = context.GetType().GetProperty("PropertyValue");
                if (pi != null)
                {
                    object obj = pi.GetValue(context, null);
                    return obj;
                }
            }

            object o = base.ConvertFrom(context, culture, value);
            return o;
        }
    }

    #endregion
}