﻿namespace GitVersionTask
{
    using GitVersion;

    public static class VersionAndBranchFinder
    {

        public static bool TryGetVersion(string directory, out SemanticVersion versionAndBranch)
        {
            var gitDirectory = GitDirFinder.TreeWalkForGitDir(directory);

            if (string.IsNullOrEmpty(gitDirectory))
            {
                var message =
                    "No .git directory found in provided solution path. This means the assembly may not be versioned correctly. " +
                    "To fix this warning either clone the repository using git or remove the `GitVersion.Fody` nuget package. " +
                    "To temporarily work around this issue add a AssemblyInfo.cs with an appropriate `AssemblyVersionAttribute`." +
                    "If it is detected that this build is occurring on a CI server an error may be thrown.";
                Logger.WriteWarning(message);
                versionAndBranch = null;
                return false;
            }

            var arguments = new Arguments();
            foreach (var buildServer in BuildServerList.GetApplicableBuildServers(arguments))
            {
                Logger.WriteInfo(string.Format("Executing PerformPreProcessingSteps for '{0}'.", buildServer.GetType().Name));
                buildServer.PerformPreProcessingSteps(gitDirectory);
            }
            versionAndBranch = VersionCache.GetVersion(gitDirectory);
            return true;
        }
    }
}