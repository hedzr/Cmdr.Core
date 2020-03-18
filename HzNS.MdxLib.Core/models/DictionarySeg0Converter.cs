using System;
using System.ComponentModel;
using System.Globalization;

namespace HzNS.MdxLib.models
{
    #region DictionarySeg0Converter

    public class DictionarySeg0Converter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(MDictIndex))
                return true;
            if (destinationType == typeof(MDictKwIndexTable))
                return true;
            if (destinationType == typeof(MDictContentIndexTable))
                return true;
            //if (destinationType == typeof(DictionarySeg3))
            //    return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
            object value, Type destinationType)
        {
            if (destinationType == typeof(String))
            {
                if (value is MDictIndex)
                {
                    MDictIndex so = (MDictIndex) value;
                    return string.Format("{1} items", 0, so.Seg1Length);
                }

                if (value is MDictKwIndexTable)
                {
                    MDictKwIndexTable so = (MDictKwIndexTable) value;
                    return string.Format("{1} items, {0} bytes", so.RawCount, so.IndexList.Length);
                }

                if (value is MDictContentIndexTable)
                {
                    MDictContentIndexTable so = (MDictContentIndexTable) value;
                    return string.Format("{1} items, {0} bytes", so.RawCount, so.Indexes.Length);
                }

                //if (value is DictionarySeg3)
                //{
                //    DictionarySeg3 so = (DictionarySeg3)value;
                //    return string.Format("{0} items, ZIP:{1}KB/{2}KB.", so.Count, so.ZippedLength / 1024, so.UnzippedLength / 1024);
                //}
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            //if (sourceType == typeof(string))
            //    return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture,
            object value)
        {
            return base.ConvertFrom(context, culture, value);
        }
    }

    #endregion
}