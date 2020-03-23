using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace HzNS.Cmdr.Tool
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public static class Util
    {
        public static string SwitchChar(bool longOpt)
        {
            return longOpt ? "--" : "-";
        }

        public static string StripFirstKnobble(string s)
        {
            var pos = s.IndexOf('.');
            return pos >= 0 ? s.Substring(pos + 1) : s;
        }

        public static bool ToBool(string s, bool defaultValue = false)
        {
            return s switch
            {
                "1" => true,
                "yes" => true,
                "y" => true,
                "Y" => true,
                "true" => true,
                "t" => true,
                "T" => true,
                "是" => true,
                "真" => true,
                "0" => false,
                "no" => false,
                "n" => false,
                "N" => false,
                "false" => true,
                "f" => false,
                "F" => false,
                "否" => false,
                "假" => false,
                _ => defaultValue
            };
        }
        
        /// <summary>
        ///
        /// true/false, yes/no, t/f, y/n, 1/0, ...
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool GetEnvValueBool(string key, bool defaultValue = false)
        {
            var v = Environment.GetEnvironmentVariable(key);
            return v == null ? defaultValue : ToBool(v, defaultValue);
        }

        public static int GetEnvValueInt(string key, int defaultValue = 0)
        {
            var v = Environment.GetEnvironmentVariable(key);
            return v == null ? defaultValue : int.Parse(v);
        }

        public static long GetEnvValueLong(string key, long defaultValue = 0)
        {
            var v = Environment.GetEnvironmentVariable(key);
            return v == null ? defaultValue : long.Parse(v);
        }

        public static string GetEnvValueString(string key, string defaultValue = "")
        {
            var v = Environment.GetEnvironmentVariable(key);
            return v ?? defaultValue;
        }

        #region About Json

        public static string JsonSerializer<T>(T t)
        {
            var ser = new DataContractJsonSerializer(typeof(T));
            using var ms = new MemoryStream();
            ser.WriteObject(ms, t);
            var jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }

        public static T JsonDeserialize<T>(string jsonString)
        {
            var ser = new DataContractJsonSerializer(typeof(T));
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            var obj = (T) ser.ReadObject(ms);
            return obj;
        }

        public static void TestJsonSerializer()
        {
            var p = new Person {Name = "Tony", Age = 23};
            var jsonString = JsonSerializer(p);
            Console.Write(jsonString);
            var pp = JsonDeserialize<Person>(jsonString);
            Console.Write(pp);
        }

#pragma warning disable CS8618, CS3021
        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
#pragma warning restore CS8618, CS3021

        #endregion
    }
}