namespace NRules.RuleSharp.IntegrationTests.TestAssets;

public abstract class BaseRuleTestFixture
{
    protected BaseRuleTestFixture()
    {
        Repository.AddNamespace("System");
        Repository.AddNamespace("NRules.RuleSharp.IntegrationTests.TestAssets");
        Repository.AddReference(typeof(BaseRuleTestFixture).Assembly);
    }

    protected RuleRepository Repository { get; } = new RuleRepository();
}