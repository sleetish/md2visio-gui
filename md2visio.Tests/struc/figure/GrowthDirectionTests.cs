using md2visio.struc.figure;
using Xunit;

namespace md2visio.Tests.struc.figure
{
    public class GrowthDirectionTests
    {
        [Theory]
        [InlineData("RL", -1, 0, false)]
        [InlineData("TD", 0, -1, false)]
        [InlineData("TB", 0, -1, false)]
        [InlineData("BT", 0, 1, true)]
        [InlineData("LR", 1, 0, true)]
        public void Decide_WithValidDirections_SetsCorrectValues(string direction, int expectedH, int expectedV, bool expectedPositive)
        {
            // Arrange
            var container = new Container();
            container.Direction = direction;

            // The Container constructor creates a GrowthDirection and its Direction setter calls Decide.
            // But we want to test GrowthDirection.Decide explicitly.
            var growthDirection = new GrowthDirection(container);

            // Act
            growthDirection.Decide(container);

            // Assert
            Assert.Equal(expectedH, growthDirection.H);
            Assert.Equal(expectedV, growthDirection.V);
            Assert.Equal(expectedPositive, growthDirection.Positive);
        }

        [Fact]
        public void Decide_WithDefaultDirection_SetsLRValues()
        {
            // Arrange
            var container = new Container(); // Default direction is "LR"
            var growthDirection = new GrowthDirection(container);

            // Act
            growthDirection.Decide(container);

            // Assert
            Assert.Equal(1, growthDirection.H);
            Assert.Equal(0, growthDirection.V);
            Assert.True(growthDirection.Positive);
        }

        [Fact]
        public void Constructor_CallsDecide()
        {
            // Arrange
            var container = new Container();
            container.Direction = "BT";

            // Act
            var growthDirection = new GrowthDirection(container);

            // Assert
            Assert.Equal(0, growthDirection.H);
            Assert.Equal(1, growthDirection.V);
            Assert.True(growthDirection.Positive);
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var container = new Container();
            container.Direction = "RL";
            var growthDirection = new GrowthDirection(container);

            // Act
            var result = growthDirection.ToString();

            // Assert
            Assert.Equal("H: -1, V: 0", result);
        }

        [Fact]
        public void Positive_ReturnsTrue_WhenHIsPositive()
        {
            var container = new Container();
            var gd = new GrowthDirection(container);
            gd.H = 1;
            gd.V = 0;
            Assert.True(gd.Positive);
        }

        [Fact]
        public void Positive_ReturnsTrue_WhenVIsPositive()
        {
            var container = new Container();
            var gd = new GrowthDirection(container);
            gd.H = 0;
            gd.V = 1;
            Assert.True(gd.Positive);
        }

        [Fact]
        public void Positive_ReturnsFalse_WhenBothAreZero()
        {
            var container = new Container();
            var gd = new GrowthDirection(container);
            gd.H = 0;
            gd.V = 0;
            Assert.False(gd.Positive);
        }

        [Fact]
        public void Positive_ReturnsFalse_WhenBothAreNegative()
        {
            var container = new Container();
            var gd = new GrowthDirection(container);
            gd.H = -1;
            gd.V = -1;
            Assert.False(gd.Positive);
        }
    }
}
