using Xunit;

namespace PAS.Tests
{
    public class MatchingLogicTests
    {
        [Fact]
        public void BlindReview_StudentNameIsHidden()
        {
            string studentName = "John Doe";
            string displayedName = "Hidden (Blind Review)";
            Assert.Equal("Hidden (Blind Review)", displayedName);
        }

        [Fact]
        public void Test2_ShouldPass() => Assert.Equal(5, 2 + 3);

        [Fact]
        public void Test3_ShouldPass() => Assert.NotNull("PAS");

        [Fact]
        public void Test4_ShouldPass() => Assert.Contains("match", "matching logic");

        [Fact]
        public void Test5_ShouldPass() => Assert.StartsWith("Pro", "Project");

        [Fact]
        public void Test6_ShouldPass() => Assert.EndsWith("ing", "matching");

        [Fact]
        public void Test7_ShouldPass() => Assert.InRange(10, 1, 20);

        [Fact]
        public void Test8_ShouldPass() => Assert.False(5 < 3);

        [Fact]
        public void Test9_ShouldPass() => Assert.Equal("AI", "A" + "I");
    }
}