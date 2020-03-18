using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace HzNS.MdxLib.MDict.Tool
{
    #region class Util, Util::FromXml, Util::ToXml

    /// <summary>
    /// 工具类
    /// </summary>
    public class Util
    {
        #region Xml operations : FromXml, ToXml

        /// <summary>
        /// 从字符串转换为指定.NET类型的对象
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        public static object FromXml(string xml, System.Type objType)
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine(ObjType);
                var ser = new System.Xml.Serialization.XmlSerializer(objType);

                var re = new System.Text.RegularExpressions.Regex(@" xmlns:xsi=[^>]+>");
                xml = re.Replace(xml, ">");
                if (xml.Trim().Length <= 0) return Activator.CreateInstance(objType);

                var stringReader = new StringReader(xml);
                var xmlReader = new XmlTextReader(stringReader);
                var obj = ser.Deserialize(xmlReader);
                xmlReader.Close();
                stringReader.Close();
                return obj;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return Activator.CreateInstance(objType);
            //return new CellB1.CellB1();
        }

        /// <summary>
        /// Serializes the <i>Obj</i> to an XML string.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        public static string ToXml(object obj, System.Type objType)
        {
            var ser = new System.Xml.Serialization.XmlSerializer(objType, TargetNamespace);
            var memStream = new MemoryStream();
            var xmlWriter = new XmlTextWriter(memStream, Encoding.UTF8) {Namespaces = true};
            ser.Serialize(xmlWriter, obj, GetNamespaces());
            xmlWriter.Close();
            memStream.Close();

            var xml = Encoding.UTF8.GetString(memStream.GetBuffer());
            xml = xml.Substring(xml.IndexOf(Convert.ToChar(60)));
            xml = xml.Substring(0, (xml.LastIndexOf(Convert.ToChar(62)) + 1));
            return xml;
        }

        #region internal attributes

        /// <summary>
        /// helper
        /// </summary>
        /// <returns></returns>
        public static System.Xml.Serialization.XmlSerializerNamespaces GetNamespaces()
        {
            System.Xml.Serialization.XmlSerializerNamespaces ns;
            ns = new System.Xml.Serialization.XmlSerializerNamespaces();
            ns.Add("", "http://www.w3.org/2001/XMLSchema");
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            return ns;
        }

        /// <summary>
        /// helper: Returns the target namespace for the serializer.
        /// </summary>
        public static string TargetNamespace
        {
            get { return "http://www.w3.org/2001/XMLSchema"; }
        }

        #endregion

        #endregion
    }

    #endregion
}