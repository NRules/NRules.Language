using System.Linq;
using NRules.RuleSharp.IntegrationTests.TestAssets;
using Xunit;

namespace NRules.RuleSharp.IntegrationTests;

public class RuleCompilerTests : BaseRuleTestFixture
{
    [Fact]
    public void Compile_EmptyFile_Succeeds()
    {
        var text = "";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void Metadata_TwoRulesWithMetadata_Loads()
    {
        var text = @"
using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule ""Metadata Rule 1""
    description ""Rule with description, priority and repeatability""
    priority 10
    repeatable false
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
        Repository.Compile();

        var rules = Repository.GetRules().ToArray();
        Assert.Equal(2, rules.Length);
        Assert.Equal("Metadata Rule 1", rules[0].Name);
        Assert.Equal("Rule with description, priority and repeatability", rules[0].Description);
        Assert.Equal(10, rules[0].Priority);
        Assert.Equal(RuleModel.RuleRepeatability.NonRepeatable, rules[0].Repeatability);
        Assert.Equal("Metadata Rule 2", rules[1].Name);
        Assert.Equal(new []{"Metadata", "Test"}, rules[1].Tags);
    }

    [Fact]
    public void Metadata_RuleWithMetadata_Loads()
    {
        var text = @"
using System;
using NRules.RuleSharp.IntegrationTests.TestAssets;

rule ""Metadata Rule""
    description ""Rule with description and repeatability""
    repeatable true
    when
        var fact = TestFact1();
    
    then
        RuleActions.NoOp();
";
        Repository.LoadText(text);
        Repository.Compile();

        var rules = Repository.GetRules().ToArray();
        Assert.Equal("Metadata Rule", rules[0].Name);
        Assert.Equal("Rule with description and repeatability", rules[0].Description);
        Assert.Equal(RuleModel.RuleRepeatability.Repeatable, rules[0].Repeatability);
    }

    [Fact]
    public void Match_Equals_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1(x => x.StringProperty == ""Valid"", x => x.StringProperty != ""Valid"");
    
then
    RuleActions.NoOp(fact);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void Match_And_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1(x => x.IntProperty >= 0 && x.IntProperty < 10);
    
then
    RuleActions.NoOp(fact);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void Match_Or_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1(x => x.IntProperty <= 0 || x.IntProperty > 10);
    
then
    RuleActions.NoOp(fact);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void Match_Boolean_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1(x => x.BoolProperty);
    
then
    RuleActions.NoOp(fact);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void Match_BooleanNot_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1(x => !x.BoolProperty);
    
then
    RuleActions.NoOp(fact);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void Match_Exists_Loads()
    {
        var text = @"
rule TestRule
when
    exists TestFact1(x => x.IntProperty > 0);
    
then
    RuleActions.NoOp();
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void Match_Not_Loads()
    {
        var text = @"
rule TestRule
when
    not TestFact1(x => x.IntProperty <= 0);
    
then
    RuleActions.NoOp();
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void MemberAccess_ArrayIndexAccess_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1();

then
    var itemLength = fact.ArrayProperty[0].Length;
    RuleActions.NoOp(fact);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void MemberAccess_ListIndexAccess_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1();

then
    var itemLength = fact.ListProperty[0].Length;
    RuleActions.NoOp(fact);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void MemberAccess_StringIndexAccess_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1();

then
    var charValue = fact.StringProperty[0];
    RuleActions.NoOp(fact);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void MemberAccess_DelegateInvoke_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1();

then
    RuleActions.GetAction()(""Test"");
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void MemberAccess_ExtensionMethod_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1();

then
    fact.ExtensionMethod();
    fact.ExtensionMethod(fact.IntProperty);
    fact.ExtensionMethod(fact.StringProperty);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void Match_JoinFact_Loads()
    {
        var text = @"
rule TestRule
when
    var fact1 = TestFact1(x => x.StringProperty == ""Valid"");
    var fact2 = TestFact2(x => x.Fact1 == fact1);
    
then
    RuleActions.NoOp(fact1, fact2);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void Match_JoinFactProperty_Loads()
    {
        var text = @"
rule TestRule
when
    var fact1 = TestFact1(x => x.StringProperty == ""Valid"");
    var fact2 = TestFact2(x => x.StringProperty == fact1.StringProperty);
    
then
    RuleActions.NoOp(fact1, fact2);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void Match_ActionNewObject_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1();
    
then
    var result = new ActionResult(""Result"");
    RuleActions.Add(result);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void Match_ActionNewObjectWithInitializer_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1();
    
then
    var result = new ActionResult{Result = ""Result""};
    RuleActions.Add(result);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void Match_ActionInsertNewFact_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1();
    
then
    var result = new ActionResult();
    Context.Insert(result);
";
        Repository.LoadText(text);
        Repository.Compile();
    }

    [Fact]
    public void Match_NumericLiterals_Loads()
    {
        var text = @"
rule TestRule
when
    var fact = TestFact1();
    
then
    var l1 = 1000L;
    var u1 = 1000U;
    var ul1 = 1000UL;
    var ul2 = 1000LU;
    var m1 = 1000.0M;
    var d1 = 1000.0D;
    var f1 = 1000.0F;
";
        Repository.LoadText(text);
        Repository.Compile();
    }
}