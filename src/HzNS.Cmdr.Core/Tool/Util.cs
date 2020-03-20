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
            if(pos>=0)
            {
                return s.Substring(pos + 1);
            }

            return s;
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