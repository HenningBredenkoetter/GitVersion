using System;
using LibGit2Sharp;
using NUnit.Framework;
using Tests.Helpers;

[TestFixture]
public class CommitCountingFixture : Lg2sHelperBase
{
    /*
        * commit 4d65c519f88773854f9345eaf5dbb30cb49f6a74 (HEAD, develop)
        |
        |     P
        |
        *   commit 7655537837096d925a4f974232f78ec589d86ebd
        |\  Merge: 0b7a248 253e943
        | |
        | |     Merge branch 'hotfix-1.3.1' into develop
        | |
        * |   commit 0b7a2482ab7d167cefa4ecfc106db001dc5c17ff
        |\ \  Merge: 243f56d 1b9cff4
        | | |
        | | |     Merge branch 'feature' into develop
        | | |
        | * | commit 1b9cff4589e2f37bc624a18c349b7d95c360aaf7 (feature)
        | | |
        | | |     K
        | | |
        | * | commit 0491c5dac30d706f4e54c5cb26d082baad8228d1
        | | |
        | | |     J
        | | |
        * | |   commit 243f56dcdb543688fd0a99bd3e0e72dd9a786603
        |\ \ \  Merge: 320f4b6 9ea56e0
        | |/ /
        |/| |       Merge branch 'release-1.3.0' into develop
        | | |
        * | | commit 320f4b6820cf4b0853dc08ac153f04fbd4958200
        | | |
        | | |     E
        | | |
        | | | *   commit 3b012518e0c89bb753459912738604a915cd70d6 (tag: 1.3.1, master)
        | | | |\  Merge: 5b84136 253e943
        | | | |/
        | | |/|       Merge branch 'hotfix-1.3.1'
        | | | |
        | | * | commit 253e94347a96fe4a1eab4e47972afd16f6992528 (hotfix-1.3.1)
        | | |/
        | | |       L
        | | |
        | | *   commit 5b84136c848fd48f1f8b3fa4e1b767a1f6101279 (tag: 1.3.0)
        | | |\  Merge: 576a28e 9ea56e0
        | | |/
        | |/|       Merge branch 'release-1.3.0'
        | | |
        | * | commit 9ea56e0287af87d6b80fad6425949ee6a92fd4c8 (release-1.3.0)
        | | |
        | | |     G
        | | |
        | * | commit b53054c614d36edc9d1bee8c35cd2ed575a43607
        |/ /
        | |       D
        | |
        * | commit fab69e28ee35dd912c0c95d5993dd84e4f2bcd92
        | |
        | |     B
        | |
        | *   commit 576a28e321cd6dc764b52c5fface672fa076f37f (tag: 1.2.1)
        | |\  Merge: 8c89048 e480263
        |/ /
        | |       Merge branch 'hotfix-1.2.1'
        | |
        | * commit e480263d0b3eeb3a35e6559032a0fdcb9eb19baa (hotfix-1.2.1)
        |/
        |       C
        |
        * commit 8c890487ed143d5a72d151e64be1c5ddb314c908 (tag: 1.2.0)

              A
    */

    /*
     * hotfix1    -C--       ---L-
     *           /    \     /     \
     * master  -A------F---H-------N
     *                    /
     * release      D----G 
     *             /       \
     * develop --B----E-----I-----M----O--P
     *                 \         /
     * feature          -----J-K-
     */
    [Test]
    public void tada()
    {
        var repoPath = Clone(CCTestRepoWorkingDirPath);

        using (var repo = new Repository(repoPath))
        {
            ResetToP(repo);
            Dump("P", repo);

            ResetToO(repo);
            Dump("O", repo);

            ResetToN(repo);
            Dump("N", repo);

            ResetToM(repo);
            Dump("M", repo);

            ResetToL(repo);
            Dump("L", repo);

            ResetToK(repo);
            Dump("K", repo);

            ResetToJ(repo);
            Dump("J", repo);

            ResetToI(repo);
            Dump("I", repo);

            ResetToH(repo);
            Dump("H", repo);

            ResetToG(repo);
            Dump("G", repo);

            ResetToF(repo);
            Dump("F", repo);

            ResetToE(repo);
            Dump("E", repo);

            ResetToD(repo);
            Dump("D", repo);

            ResetToC(repo);
            Dump("C", repo);

            ResetToB(repo);
            Dump("B", repo);
        }
    }

    void DropTags(Repository repo, params string[] names)
    {
        foreach (var name in names)
        {
            if (repo.Tags[name] == null)
            {
                continue;
            }

            repo.Tags.Remove(name);
        }
    }

    void DropBranches(Repository repo, params string[] names)
    {
        foreach (var name in names)
        {
            if (repo.Branches[name] == null)
            {
                continue;
            }

            repo.Branches.Remove(name);
        }
    }

    void ResetBranch(Repository repo, string name, string committish)
    {
        var b = repo.Branches[name];
        Assert.IsNotNull(b);
        repo.Refs.UpdateTarget(b.CanonicalName, committish);
    }

    void ResetToP(Repository repo)
    {
        ResetBranch(repo, "develop", "4d65c519f88773854f9345eaf5dbb30cb49f6a74");
    }

    void ResetToO(Repository repo)
    {
        ResetBranch(repo, "develop", "7655537837096d925a4f974232f78ec589d86ebd");
    }

    void ResetToN(Repository repo)
    {
        ResetBranch(repo, "develop", "0b7a2482ab7d167cefa4ecfc106db001dc5c17ff");
    }

    void ResetToM(Repository repo)
    {
        ResetBranch(repo, "develop", "0b7a2482ab7d167cefa4ecfc106db001dc5c17ff");
        ResetBranch(repo, "master", "5b84136c848fd48f1f8b3fa4e1b767a1f6101279");
        DropTags(repo, "1.3.1");
    }

    void ResetToL(Repository repo)
    {
        ResetBranch(repo, "develop", "243f56dcdb543688fd0a99bd3e0e72dd9a786603");
    }

    void ResetToK(Repository repo)
    {
        DropBranches(repo, "hotfix-1.3.1");
    }

    void ResetToJ(Repository repo)
    {
        ResetBranch(repo, "feature", "0491c5dac30d706f4e54c5cb26d082baad8228d1");
    }

    void ResetToI(Repository repo)
    {
        DropBranches(repo, "feature");
    }

    void ResetToH(Repository repo)
    {
        ResetBranch(repo, "develop", "320f4b6820cf4b0853dc08ac153f04fbd4958200");
    }
    /*
    hotfix-1.2.1       -----------C--      
                      /              \     
    master           A----------------F-----H-------N
                      \                    / \     /
    hotfix-1.3.1       \                  /   ----L
                        \                /         \
    release-1.3.0        \        -D----G---        \
                          \      /          \        \
    develop                -----B----E-------I-----M--O--P
                                      \           /
    feature                            -------J-K-
    */
    void ResetToG(Repository repo)
    {
        ResetBranch(repo, "master", "576a28e321cd6dc764b52c5fface672fa076f37f");
        DropTags(repo, "1.3.0");
    }

    void ResetToF(Repository repo)
    {
        ResetBranch(repo, "release-1.3.0", "b53054c614d36edc9d1bee8c35cd2ed575a43607");
    }

    void ResetToE(Repository repo)
    {
        ResetBranch(repo, "master", "8c890487ed143d5a72d151e64be1c5ddb314c908");
        DropTags(repo, "1.2.1");
    }

    void ResetToD(Repository repo)
    {
        ResetBranch(repo, "develop", "fab69e28ee35dd912c0c95d5993dd84e4f2bcd92");
    }

    void ResetToC(Repository repo)
    {
        DropBranches(repo, "release-1.3.0");
    }

    void ResetToB(Repository repo)
    {
        DropBranches(repo, "hotfix-1.2.1");
    }

    static void Dump(string step, Repository repo)
    {
        Console.WriteLine("step " + step);
        Console.WriteLine("---");
        foreach (var branch in repo.Branches)
        {
            Console.WriteLine(branch.CanonicalName + " -> " + branch.Tip.Sha);
        }

        foreach (var tag in repo.Tags)
        {
            Console.WriteLine(tag.CanonicalName + " -> " + tag.Target.Sha);
        }

        Console.WriteLine();
    }
}