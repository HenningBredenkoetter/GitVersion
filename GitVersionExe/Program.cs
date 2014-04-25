namespace GitVersion
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    class Program
    {
        const string MsBuild = @"c:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe";

        static void Main()
        {
            int? exitCode = null;

            try
            {
                Arguments arguments;
                var argumentsWithoutExeName = GetArgumentsWithoutExeName();
                try
                {
                    arguments = ArgumentParser.ParseArguments(argumentsWithoutExeName);
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to parse arguments: {0}", string.Join(" ", argumentsWithoutExeName));

                    HelpWriter.Write();
                    return;
                }

                if (arguments.IsHelp)
                {
                    HelpWriter.Write();
                    return;
                }

                ConfigureLogging(arguments);

                var gitPreparer = new GitPreparer(arguments);
                var gitDirectory = gitPreparer.Prepare();
                if (string.IsNullOrEmpty(gitDirectory))
                {
                    Console.Error.WriteLine("Failed to prepare or find the .git directory in path '{0}'", arguments.TargetPath);
                    Environment.Exit(1);
                }

                var workingDirectory = Directory.GetParent(gitDirectory).FullName;
                var applicableBuildServers = GetApplicableBuildServers(arguments).ToList();

                foreach (var buildServer in applicableBuildServers)
                {
                    buildServer.PerformPreProcessingSteps(gitDirectory);
                }

                var semanticVersion = VersionCache.GetVersion(gitDirectory);

                if (arguments.Output == OutputType.BuildServer)
                {
                    foreach (var buildServer in applicableBuildServers)
                    {
                        buildServer.WriteIntegration(semanticVersion, Console.WriteLine);
                    }
                }

                var variables = VariableProvider.GetVariablesFor(semanticVersion);
                if (arguments.Output == OutputType.Json)
                {
                    switch (arguments.VersionPart)
                    {
                        case null:
                            Console.WriteLine(JsonOutputFormatter.ToJson(variables));
                            break;

                        default:
                            string part;
                            if (!variables.TryGetValue(arguments.VersionPart, out part))
                            {
                                throw new ErrorException(string.Format("Could not extract '{0}' from the available parts.", arguments.VersionPart));
                            }
                            Console.WriteLine(part);
                            break;
                    }
                }

                using (var assemblyInfoUpdate = new AssemblyInfoFileUpdate(arguments, workingDirectory, variables))
                {
                    var execRun = RunExecCommandIfNeeded(arguments, workingDirectory, variables);
                    var msbuildRun = RunMsBuildIfNeeded(arguments, workingDirectory, variables);
                    if (!execRun && !msbuildRun)
                    {
                        assemblyInfoUpdate.DoNotRestoreAssemblyInfo();
                        //TODO Put warning back
                        //if (!context.CurrentBuildServer.IsRunningInBuildAgent())
                        //{
                        //    Console.WriteLine("WARNING: Not running in build server and /ProjectFile or /Exec arguments not passed");
                        //    Console.WriteLine();
                        //    Console.WriteLine("Run GitVersion.exe /? for help");
                        //}
                    }
                }

                if (gitPreparer.IsDynamicGitRepository)
                {
                    DeleteHelper.DeleteGitRepository(gitPreparer.DynamicGitRepositoryPath);
                }
            }
            catch (ErrorException exception)
            {
                var error = string.Format("An error occurred:\r\n{0}", exception.Message);
                Logger.WriteError(error);

                exitCode = 1;
            }
            catch (Exception exception)
            {
                var error = string.Format("An unexpected error occurred:\r\n{0}", exception);
                Logger.WriteWarning(error);

                exitCode = 1;
            }

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }

            if (!exitCode.HasValue)
            {
                exitCode = 0;
            }

            Environment.Exit(exitCode.Value);
        }

        static IEnumerable<IBuildServer> GetApplicableBuildServers(Arguments arguments)
        {
            return BuildServerList.GetApplicableBuildServers(arguments);
        }

        static void ConfigureLogging(Arguments arguments)
        {
            Action<string> writeAction = x => { };

            if (arguments.LogFilePath == "console")
            {
                writeAction = Console.WriteLine;
            }
            else if (arguments.LogFilePath != null)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(arguments.LogFilePath));
                    if (File.Exists(arguments.LogFilePath))
                    {
                        using (File.CreateText(arguments.LogFilePath)) { }
                    }

                    writeAction = x => WriteLogEntry(arguments, x);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to configure logging: " + ex.Message);
                }
            }

            Logger.WriteInfo = writeAction;
            Logger.WriteWarning = writeAction;
            Logger.WriteError = writeAction;
        }

        static void WriteLogEntry(Arguments arguments, string s)
        {
            var contents = string.Format("{0}\t\t{1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), s);
            File.AppendAllText(arguments.LogFilePath, contents);
        }

        static List<string> GetArgumentsWithoutExeName()
        {
            return Environment.GetCommandLineArgs()
                .Skip(1)
                .ToList();
        }

        static bool RunMsBuildIfNeeded(Arguments args, string workingDirectory, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(args.Proj)) return false;

            Logger.WriteInfo(string.Format("Launching {0} \"{1}\" {2}", MsBuild, args.Proj, args.ProjArgs));
            var results = ProcessHelper.Run(
                Logger.WriteInfo, Logger.WriteError,
                null, MsBuild, string.Format("\"{0}\" {1}", args.Proj, args.ProjArgs), workingDirectory,
                GetEnvironmentalVariables(variables));

            if (results != 0)
                throw new ErrorException("MsBuild execution failed, non-zero return code");

            return true;
        }

        static bool RunExecCommandIfNeeded(Arguments args, string workingDirectory, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(args.Exec)) return false;

            Logger.WriteInfo(string.Format("Launching {0} {1}", args.Exec, args.ExecArgs));
            var results = ProcessHelper.Run(
                Logger.WriteInfo, Logger.WriteError,
                null, args.Exec, args.ExecArgs, workingDirectory,
                GetEnvironmentalVariables(variables));
            if (results != 0)
                throw new ErrorException(string.Format("Execution of {0} failed, non-zero return code", args.Exec));

            return true;
        }

        static KeyValuePair<string, string>[] GetEnvironmentalVariables(Dictionary<string, string> variables)
        {
            return variables
                .Select(v => new KeyValuePair<string, string>("GitVersion_" + v.Key, v.Value))
                .ToArray();
        }
    }
}