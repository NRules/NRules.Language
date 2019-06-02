using NRules.RuleSharp.IntegrationTests.TestAssets;
using Xunit;

namespace NRules.RuleSharp.IntegrationTests
{
    public class RuleCompilerErrorTests : BaseRuleTestFixture
    {
        [Fact]
        public void Rule_MissingRuleName_ThrowsWithSourceLocation()
        {
            var text = @"
rule
when
    var fact = TestFact1(x => x.BoolProperty);
    
then
    RuleActions.NoOp(fact);
";
            var ex = Assert.Throws<RulesParseException>(() => Repository.LoadText(text));
            Assert.Equal(2, ex.Location.LineNumber);
            Assert.Equal(0, ex.Location.ColumnNumber);
        }
        
        [Fact]
        public void Rule_MalformedRuleName_ThrowsWithSourceLocation()
        {
            var text = @"
rule Test Rule
when
    var fact = TestFact1(x => x.BoolProperty);
    
then
    RuleActions.NoOp(fact);
";
            var ex = Assert.Throws<RulesParseException>(() => Repository.LoadText(text));
            Assert.Equal(2, ex.Location.LineNumber);
            Assert.Equal(0, ex.Location.ColumnNumber);
        }

        [Fact]
        public void Rule_MissingRightHandSide_ThrowsWithSourceLocation()
        {
            var text = @"
rule TestRule
when
    var fact = TestFact1(x => x.BoolProperty);
";
            var ex = Assert.Throws<RulesParseException>(() => Repository.LoadText(text));
            Assert.Equal(3, ex.Location.LineNumber);
            Assert.Equal(0, ex.Location.ColumnNumber);
        }

        [Fact]
        public void Rule_MissingLeftHandSide_ThrowsWithSourceLocation()
        {
            var text = @"
rule TestRule
then
    RuleActions.NoOp();
";
            var ex = Assert.Throws<RulesParseException>(() => Repository.LoadText(text));
            Assert.Equal(2, ex.Location.LineNumber);
            Assert.Equal(0, ex.Location.ColumnNumber);
        }

        [Fact]
        public void Match_UnknownType_ThrowsWithSourceLocation()
        {
            var text = @"
rule TestRule
when
    var fact = TestFactZ();
    
then
    RuleActions.NoOp();
";
            var ex = Assert.Throws<RulesParseException>(() => Repository.LoadText(text));
            Assert.StartsWith("Unknown type. Type=TestFactZ", ex.Message);
            Assert.Equal(4, ex.Location.LineNumber);
            Assert.Equal(4, ex.Location.ColumnNumber);
        }

        [Fact]
        public void Match_UnknownMember_ThrowsWithSourceLocation()
        {
            var text = @"
rule TestRule
when
    var fact = TestFact1(x => x.MemberZ == 0);
    
then
    RuleActions.NoOp();
";
            var ex = Assert.Throws<RulesParseException>(() => Repository.LoadText(text));
            Assert.StartsWith("Type member not found. Type=NRules.RuleSharp.IntegrationTests.TestAssets.TestFact1, Member=MemberZ", ex.Message);
            Assert.Equal(4, ex.Location.LineNumber);
            Assert.Equal(31, ex.Location.ColumnNumber);
        }

        [Fact]
        public void Action_UnknownType_ThrowsWithSourceLocation()
        {
            var text = @"
rule TestRule
when
    var fact = TestFact1();
    
then
    RuleActionsZ.NoOp();
";
            var ex = Assert.Throws<RulesParseException>(() => Repository.LoadText(text));
            Assert.StartsWith("Unknown identifier. Identifier=RuleActionsZ.NoOp", ex.Message);
            Assert.Equal(7, ex.Location.LineNumber);
            Assert.Equal(21, ex.Location.ColumnNumber);
        }

        [Fact]
        public void Action_UnknownIdentifier_ThrowsWithSourceLocation()
        {
            var text = @"
rule TestRule
when
    var fact = TestFact1();
    
then
    RuleActions.NoOp(factZ);
";
            var ex = Assert.Throws<RulesParseException>(() => Repository.LoadText(text));
            Assert.StartsWith("Unknown identifier. Identifier=factZ", ex.Message);
            Assert.Equal(7, ex.Location.LineNumber);
            Assert.Equal(21, ex.Location.ColumnNumber);
        }

        [Fact]
        public void Action_UnknownMethod_ThrowsWithSourceLocation()
        {
            var text = @"
rule TestRule
when
    var fact = TestFact1();
    
then
    RuleActions.MethodZ(fact);
";
            var ex = Assert.Throws<RulesParseException>(() => Repository.LoadText(text));
            Assert.StartsWith("Type member not found. Type=NRules.RuleSharp.IntegrationTests.TestAssets.RuleActions, Member=MethodZ", ex.Message);
            Assert.Equal(7, ex.Location.LineNumber);
            Assert.Equal(15, ex.Location.ColumnNumber);
        }
    }
}
