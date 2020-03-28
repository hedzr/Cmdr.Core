using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Tests.Tool;
using Xunit;

namespace Tests
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
        }

        [Fact]
        public void Add_SingleNumber_ReturnsSameNumber()
        {
            var stringCalculator = new StringCalculator();

            var actual = stringCalculator.Add("0");

            Assert.Equal(0, actual);
        }

        [Fact]
        public void Add_MaximumSumResult_ThrowsOverflowException()
        {
            var stringCalculator = new StringCalculator();
            const string maximumResult = "4623784623462364283467238472361001";

            void Actual() => stringCalculator.Add(maximumResult);

            Assert.Throws<OverflowException>(Actual);
        }

        private class StringCalculator
        {
            private int _value;

            public int Add(string s)
            {
                // throw new NotImplementedException();
                var x = ToInt(s);
                _value += x;
                return _value;
            }

            private static int ToInt(string s)
            {
                if (int.TryParse(s, out var res))
                    return res;
                throw new OverflowException($"{s} is overflow.");
            }
        }

        [Theory]
        [InlineData(0, 2, 2)]
        [InlineData(1, 2, 3)]
        [InlineData(-1, 2, 1)]
        [InlineData(-1, -2, -3)]
        public void Test2(int a, int b, int excepted)
        {
            Assert.True(NumberCalculation.Add(a, b) == excepted, $"{a} + {b} Should be {excepted}");
        }

        private class NumberCalculation
        {
            public static int Add(int a, int b)
            {
                return a + b;
            }
        }
    }

    [SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
    public class UnitTest2
    {
        [Fact]
        public void Test1()
        {
            MainX(null);
        }

        // ReSharper disable once UnusedParameter.Local
        static void MainX(string[] args)
        {
            foreach (var item in FilterWithoutYield())
            {
                Console.WriteLine(item); //3，4，5
            }

            //可以用ToList()从IEnumerable<out T>创建一个List<T>,并获得长度3
            Console.WriteLine(FilterWithoutYield().ToList().Count());
            // Console.ReadLine();
        }

        static List<int> Data()
        {
            return new List<int> {1, 2, 3, 4, 5};
        }

        //这种传统方式需要额外创建一个List<int> 增加开销，而且需要把Data()全部加载到内存才能再遍历。
        static IEnumerable<int> FilterWithoutYield()
        {
            List<int> result = new List<int>();
            foreach (int i in Data())
            {
                if (i > 2)
                    result.Add(i);
            }

            return result;
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once ArrangeTypeMemberModifiers
        static IEnumerable<int> FilterWithYield()
        {
            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            foreach (int i in Data())
            {
                if (i > 2)
                    yield return i;
            }

            // ReSharper disable once RedundantJumpStatement
            yield break; // 迭代器代码使用 yield return 语句依次返回每个元素，yield break将终止迭代。
        }
    }


    public class UnitTest3
    {
        [Fact]
        public void Test1()
        {
            SingletonTest(null);
        }

        // ReSharper disable once UnusedParameter.Local
        private static void SingletonTest(string[] args)
        {
            // The client code.

            Console.WriteLine(
                "{0}\n{1}\n\n{2}\n",
                "If you see the same value, then singleton was reused (yay!)",
                "If you see different values, then 2 singletons were created (booo!!)",
                "RESULT:"
            );

            var process1 = new Thread(() => { TestSingleton("FOO"); });
            var process2 = new Thread(() => { TestSingleton("BAR"); });

            process1.Start();
            process2.Start();

            process1.Join();
            process2.Join();

            Console.WriteLine("UnitTest3 ok");
        }

        private static void TestSingleton(string value)
        {
            var singleton = Singleton.GetInstance(value);
            Console.WriteLine(singleton.Value);
        }
    }
}