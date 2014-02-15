namespace GitFlowVersion
{
    using System.Linq;
    using LibGit2Sharp;

    class DevelopVersionFinder
    {
        public VersionAndBranch FindVersion(GitVersionContext context)
        {
            var version = GetSemanticVersion(context);

            version.Minor++;
            version.Patch = 0;

            return new VersionAndBranch
                   {
                       BranchType = BranchType.Develop,
                       BranchName = "develop",
                       Sha = context.CurrentBranch.Tip.Sha,
                       Version = version
                   };
        }

        SemanticVersion GetSemanticVersion(GitVersionContext context)
        {
            var tip = context.CurrentBranch.Tip;

            var versionOnMasterFinder = new VersionOnMasterFinder();
            var versionFromMaster = versionOnMasterFinder.Execute(context, tip.When());

            var f = new CommitFilter
            {
                Since = tip,
                Until = context.Repository.FindBranch("master").Tip
            };

            var c = context.Repository.Commits.QueryBy(f);
            var preReleasePartOne = c.Count();

            return new SemanticVersion
            {
                Major = versionFromMaster.Major,
                Minor = versionFromMaster.Minor,
                Tag = Stability.Unstable.ToString().ToLower() + preReleasePartOne
            };
        }
    }
}
