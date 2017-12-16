using System.Linq;
using NRules.RuleSharp.IntegrationTests.TestAssets;
using Xunit;

namespace NRules.RuleSharp.IntegrationTests
{
    public class RuleCompilerTests : BaseRuleTestFixture
    {
        [Fact]
        public void Metadata_TwoRulesWithMetadata_Loads()
        {
            var text = @"
using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule ""Metadata Rule 1""
    description ""Rule with description and priority""
    priority 10
    when
        var fact = TestFact1();
    
    then
        RuleActions.NoOp();

rule ""Metadata Rule 2""
{
    tag ""Metadata"", ""Test""
    when
        var fact = TestFact1();
    
    then
        RuleActions.NoOp();
}
";
            Repository.LoadText(text);

            var rules = Repository.GetRules().ToArray();
            Assert.Equal(2, rules.Length);
            Assert.Equal("Metadata Rule 1", rules[0].Name);
            Assert.Equal("Rule with description and priority", rules[0].Description);
            Assert.Equal(10, rules[0].Priority);
            Assert.Equal("Metadata Rule 2", rules[1].Name);
            Assert.Equal(new []{"Metadata", "Test"}, rules[1].Tags);
        }

        [Fact]
        public void Match_RulesWithFactConditions_Loads()
        {
            var text = @"
using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule TestRule1
when
    var fact = TestFact1(x => x.StringProperty == ""Valid"" && x.IntProperty >= 0, x => x.BoolProperty);
    
then
    RuleActions.NoOp();

rule TestRule2
when
    var fact = TestFact1(x => x.StringProperty != ""Valid"" || x.IntProperty < 0, x => !x.BoolProperty);
    
then
    RuleActions.NoOp();
";
            Repository.LoadText(text);
        }

        [Fact]
        public void Match_Exists_Loads()
        {
            var text = @"
using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule TestRule
when
    exists TestFact1(x => x.IntProperty > 0);
    
then
    RuleActions.NoOp();
";
            Repository.LoadText(text);
        }

        [Fact]
        public void Match_Not_Loads()
        {
            var text = @"
using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule TestRule
when
    not TestFact1(x => x.IntProperty <= 0);
    
then
    RuleActions.NoOp();
";
            Repository.LoadText(text);
        }

        [Fact]
        public void MemberAccess_ArrayIndexAccess_Loads()
        {
            var text = @"
using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule TestRule
when
    var fact = TestFact1();

then
    var itemLength = fact.ArrayProperty[0].Length;
    RuleActions.NoOp();
";
            Repository.LoadText(text);
        }

        [Fact]
        public void MemberAccess_ListIndexAccess_Loads()
        {
            var text = @"
using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule TestRule
when
    var fact = TestFact1();

then
    var itemLength = fact.ListProperty[0].Length;
    RuleActions.NoOp();
";
            Repository.LoadText(text);
        }

        [Fact]
        public void MemberAccess_StringIndexAccess_Loads()
        {
            var text = @"
using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule TestRule
when
    var fact = TestFact1();

then
    var charValue = fact.StringProperty[0];
    RuleActions.NoOp();
";
            Repository.LoadText(text);
        }

        [Fact]
        public void MemberAccess_DelegateInvoke_Loads()
        {
            var text = @"
using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule TestRule
when
    var fact = TestFact1();

then
    RuleActions.GetAction()(""Test"");
";
            Repository.LoadText(text);
        }
    }
}
