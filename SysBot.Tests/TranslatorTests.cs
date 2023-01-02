using FluentAssertions;
using PKHeX.Core;
using SysBot.Pokemon;
using Xunit;

namespace SysBot.Tests
{
    public class TranslatorTests
    {
        [Theory]
        [InlineData("公肯泰罗帕底亚的样子（火）形态", "Tauros-Paldea-Fire (M)")]
        public void TestForm(string input, string output)
        {
            var result = ShowdownTranslator<PK9>.Chinese2Showdown(input);
            result.Should().Be(output);
        }

    }
}
