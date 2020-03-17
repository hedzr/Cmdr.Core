using System;
using System.IO;
using mdxlib.MDict.Tool;
using mdxlib.models;

namespace mdxlib
{
    #region Dictionary File Loader

    public abstract class Loader : IDisposable
    {
        public string DictFileName { get; private set; }

        public DictionaryXmlHeader DictHeader { get; private set; }

        protected void SetDictionaryXmlHeader(string xml)
        {
            //if(DictionaryXmlHeader==null){
            this.DictHeader = (DictionaryXmlHeader) Util.FromXml(xml, typeof(DictionaryXmlHeader));
            if (this.DictHeader == null) return;

            if (string.IsNullOrEmpty(this.DictHeader.Title) ||
                DictHeader.Title.IndexOf("No HTML", StringComparison.Ordinal) >= 0)
            {
                this.DictHeader.Title = Path.GetFileNameWithoutExtension(DictFileName);
            }

            //}
        }

        protected bool HeaderIsValidate;
        public bool IsLibraryData { get; internal set; }

        public abstract MDictIndex DictIndex { get; set; }
        public abstract MDictKwIndexTable DictKwIndexTable { get; set; }
        public abstract MDictContentIndexTable DictLargeContentIndexTable { get; set; }

        //public virtual Dictionary<string, DictItem> DictItems { get; set; }

        public virtual Loader Open(string dictFileName)
        {
            this.DictFileName = dictFileName;
            //this.DictItems = new Dictionary<string, DictItem>();
            this.DictHeader = null;
            return this;
        }

        protected virtual Loader Shutdown()
        {
            return this;
        }

        protected virtual void Log(string s)
        {
        } //自动追加回车换行

        protected virtual void LogString(string s)
        {
        }

        protected virtual void ErrorLog(string s)
        {
        }

        /// <summary>
        /// 测试字典的完整性。通过测试字典的表头属性来进行简单的确定。
        /// </summary>
        /// <returns></returns>
        public virtual bool TestIntegrity()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 打开一个字典。
        /// </summary>
        /// <returns></returns>
        public virtual bool Process()
        {
            return true;
        }

        public virtual byte[] LoadContentBytesByKeyword(KwIndex2 kwi2)
        {
            throw new NotImplementedException();
        }

        public virtual string LoadContentByKeyword(KwIndex2 kwi2)
        {
            throw new NotImplementedException();
        }

        public static Loader CreateInstance(string dictFileName)
        {
            if (!File.Exists(dictFileName))
                return null;

            var ext = Path.GetExtension(dictFileName).ToLower();
            if (ext != ".mdx" && ext != ".mdd") return null;

            Loader l = new MDict.MDictLoader();
            l.Open(dictFileName);
            return l;
        }

        #region IDisposable 成员

        public void Dispose()
        {
            Console.WriteLine("Loader.Dispose()");
            Shutdown();
        }

        #endregion
    }

    #endregion
}