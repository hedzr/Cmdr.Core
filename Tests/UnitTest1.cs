using System;
using Xunit;

namespace Tests
{
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
        void Add_MaximumSumResult_ThrowsOverflowException()
        {
            var stringCalculator = new StringCalculator();
            const string maximumResult = "4623784623462364283467238472361001";

            void Actual() => stringCalculator.Add(maximumResult);

            Assert.Throws<OverflowException>((Action) Actual);
        }

        private class StringCalculator
        {
            private int _value = 0;

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
                    return  res;
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
}