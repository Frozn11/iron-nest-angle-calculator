using System;
using Xunit;
using CalculateAngleViaDistanceIronNest.Calculate;

namespace CalculateAngleViaDistanceIronNest.Tests
{
    public class CalculatorTests
    {
        // CalcAngle

        [Theory]
        [InlineData(5f, 1, 60f)]
        [InlineData(10f, 2, 60f)]
        [InlineData(15f, 3, 60f)]
        [InlineData(30f, 6, 60f)]
        [InlineData(2.5f, 4, 7.5f)]
        [InlineData(0f, 1, 0f)]
        public void CalcAngle_ReturnsExpectedAngle(float distance, int charges, float expected)
        {
            float result = Calculator.CalcAngle(distance, charges);
            Assert.Equal(expected, result, 3);
        }

        [Fact]
        public void CalcAngle_FloorsInsteadOfRounding()
        {
            float result = Calculator.CalcAngle(1f, 7);
            Assert.Equal(1.71f, result, 3);
        }

        // CalcTimeTravel

        [Theory]
        [InlineData(10f, 1, 47.619f)]
        [InlineData(10f, 2, 38.320f)]
        [InlineData(10f, 3, 26.145f)]
        [InlineData(10f, 4, 18.957f)]
        [InlineData(10f, 5, 15.407f)]
        [InlineData(10f, 6, 14.286f)]
        public void CalcTimeTravel_ReturnsExpectedTime(float distance, int charges, float expected)
        {
            float result = Calculator.CalcTimeTravel(distance, charges);
            Assert.Equal(expected, result, 2);
        }

        // GetMinCharges

        [Theory]
        [InlineData(5f, 1)]
        [InlineData(10f, 2)]
        [InlineData(15f, 3)]
        [InlineData(20f, 4)]
        [InlineData(25f, 5)]
        [InlineData(30f, 6)]
        public void GetMinCharges_ReturnsExpectedMinimum_ForInGameRange(float km, int expectedCharges)
        {
            int result = Calculator.GetMinCharges(km);
            Assert.Equal(expectedCharges, result);
        }

        [Fact]
        public void GetMinCharges_ReturnsMinusOne_WhenDistanceTooFarForAnyValidChargeCount()
        {
            int result = Calculator.GetMinCharges(36f);
            Assert.Equal(-1, result);
        }

        [Fact]
        public void GetMinCharges_AtDistance35_ReturnsSevenNotMinusOne()
        {
            int result = Calculator.GetMinCharges(35f);
            Assert.Equal(7, result);
        }
    }
}