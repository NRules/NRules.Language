using NRules.RuleSharp.IntegrationTests.TestAssets;
using Xunit;

namespace NRules.RuleSharp.IntegrationTests
{
    public class RuleCompilerTests : BaseRuleTestFixture
    {
        [Fact]
        public void Load_SingleFactRule_Success()
        {
            Repository.Load(@"Rules\TestRuleSet1.rs");
        }
    }
}
