using System.Linq;
using NRules.RuleSharp.IntegrationTests.TestAssets;
using Xunit;

namespace NRules.RuleSharp.IntegrationTests
{
    public class RuleCompilerTests : BaseRuleTestFixture
    {
        [Fact]
        public void Load_RulesWithMetadata_Loads()
        {
            Repository.Load(@"Rules\RuleMetadataTests.rs");

            var rules = Repository.GetRules().ToArray();
            Assert.Equal(2, rules.Length);
            Assert.Equal("Metadata Rule 1", rules[0].Name);
            Assert.Equal("Rule with description and priority", rules[0].Description);
            Assert.Equal(10, rules[0].Priority);
            Assert.Equal("Metadata Rule 2", rules[1].Name);
            Assert.Equal(new []{"Metadata", "Test"}, rules[1].Tags);
        }

        [Fact]
        public void Load_RulesWithFactConditions_Loads()
        {
            Repository.Load(@"Rules\FactConditionTests.rs");
        }
    }
}
