namespace GitVersion
{
    class PullVersionFinder : DevelopBasedVersionFinderBase
    {
        public SemanticVersion FindVersion(GitVersionContext context)
        {
            var issueNumber = ExtractIssueNumber(context);

            var version = FindVersion(context, BranchType.PullRequest);
            version.PreReleaseTag = new SemanticVersionPreReleaseTag("PullRequest", int.Parse(issueNumber));
            //TODO version.Version.BuildMetaData = NumberOfCommitsOnBranchSinceCommit(context.CurrentBranch, commonAncestor);
            return version;
        }

        string ExtractIssueNumber(GitVersionContext context)
        {
            const string prefix = "/pull/";
            var pullRequestBranch = context.CurrentBranch;

            var start = pullRequestBranch.CanonicalName.IndexOf(prefix, System.StringComparison.Ordinal);
            var end = pullRequestBranch.CanonicalName.LastIndexOf("/merge", pullRequestBranch.CanonicalName.Length - 1,
                System.StringComparison.Ordinal);

            string issueNumber = null;

            if (start != -1 && end != -1 && start + prefix.Length <= end)
            {
                start += prefix.Length;
                issueNumber = pullRequestBranch.CanonicalName.Substring(start, end - start);
            }

            if (!LooksLikeAValidPullRequestNumber(issueNumber))
            {
                throw new ErrorException(string.Format("Unable to extract pull request number from '{0}'.",
                    pullRequestBranch.CanonicalName));
            }

            return issueNumber;
        }

        bool LooksLikeAValidPullRequestNumber(string issueNumber)
        {
            if (string.IsNullOrEmpty(issueNumber))
            {
                return false;
            }

            uint res;
            if (!uint.TryParse(issueNumber, out res))
            {
                return false;
            }

            return true;
        }
    }
}
