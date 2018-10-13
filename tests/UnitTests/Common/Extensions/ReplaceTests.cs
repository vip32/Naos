namespace Naos.Core.UnitTests.Common
{
    using System;
    using Naos.Core.Common;
    using Xunit;

    public class ReplaceTests
    {
        [Fact]
        public void ReplaceGivenVariedCaseString_ReplacesCorrectly()
        {
            const string s = "Hello everyone from {{RecipIenT}}";

            Assert.Equal("Hello everyone from John Doe", Extensions.Replace(s, "{{recipient}}", "John Doe", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void ReplaceGivenVariedCase_ReplacesAllOccurences()
        {
            const string s = "Hello everyone from {{RecipIenT}}. Allow me, {{reCIPIeNt}} to welcome you all to {{RECIpieNT}}'s party.";

            Assert.Equal("Hello everyone from John Doe. Allow me, John Doe to welcome you all to John Doe's party.", Extensions.Replace(s, "{{recipient}}", "John Doe", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void ReplaceGivenNonExistingTerm_ReturnsOriginalString()
        {
            const string s = "This string doesn't inlcude the term to be removed";

            Assert.Equal(s, Extensions.Replace(s, "not-here", "whatever", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void ReplaceWhenOldValue_EqualsNewValue()
        {
            const string s = "replace my OldValue here please. OldValue dafds fOldValue";

            Assert.Equal(s, Extensions.Replace(s, "OldValue", "OldValue", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void ReplaceEmptyString_WithEmptyString()
        {
            const string s = "This is a string";

            Assert.Equal(s, Extensions.Replace(s, string.Empty, string.Empty, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void ReplaceWhenOldValueLenght_IsGreaterThanSourceReturnsSource()
        {
            const string s = "œ";

            Assert.Equal(s, Extensions.Replace(s, "oe", string.Empty, StringComparison.OrdinalIgnoreCase));
        }
    }
}
