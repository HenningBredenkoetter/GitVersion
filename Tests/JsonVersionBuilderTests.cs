using ApprovalTests;
using GitVersion;
using NUnit.Framework;

[TestFixture]
public class JsonVersionBuilderTests
{
    [Test]
    public void Json()
    {
        var semanticVersion = new SemanticVersion
            {
                Major = 1,
                Minor = 2,
                Patch = 3,
                PreReleaseTag = "unstable4",
                BuildMetaData = new SemanticVersionBuildMetaData(5, "feature1", "a682956dc1a2752aa24597a0f5cd939f93614509", null, null)
            };
        var variables = VariableProvider.GetVariablesFor(semanticVersion);
        var json = JsonOutputFormatter.ToJson(variables);
        Approvals.Verify(json);
    }

}