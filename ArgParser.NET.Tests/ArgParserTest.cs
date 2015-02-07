using Xunit;

namespace ArgParser.NET.Tests
{
    public class ArgParserTest
    {
        [Fact]
        public void FallToDefaultHookWithEmptyArgs() {
            var defaultHookPopulated = false;
            var options = new CommandDef(() => defaultHookPopulated = true) {
                {'h', "Print help message", () => { }}
            };
            var extraArgs = options.Parse(new string[] {});
            Assert.Empty(extraArgs);
            Assert.True(defaultHookPopulated);
        }
    }
}
