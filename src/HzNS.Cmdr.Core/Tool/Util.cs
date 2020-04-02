#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using HzNS.Cmdr.Tool.Ext;

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
            return v?.ToBool(defaultValue) ?? defaultValue;
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

        public static T GetEnvValue<T>(IEnumerable<string> key, params T[] defaultValues)
        {
            var k = string.Join("_", key);
            return GetEnvValue(k, defaultValues);
        }

        public static T GetEnvValue<T>(string key, params T[] defaultValues)
        {
            var v = Environment.GetEnvironmentVariable(key);
#pragma warning disable CS8603,CS8653
            if (string.IsNullOrWhiteSpace(v)) return defaultValues.Length > 0 ? defaultValues[^1] : default(T);
#pragma warning restore CS8603,CS8653

            if (typeof(T) == typeof(string))
                return (T) (object) v;
            return (T) Convert.ChangeType(v, typeof(T));
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