// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.DotNet.DarcLib;

public interface ILocalGitRepo : IGitRepo
{
    /// <summary>
    ///     Add a remote to a local repo if does not already exist, and attempt to fetch commits.
    /// </summary>
    /// <param name="repoDir">Path to a git repository</param>
    /// <param name="repoUrl">URL of the remote to add</param>
    /// <param name="skipFetch">Skip fetching remote changes</param>
    /// <returns>Name of the remote</returns>
    string AddRemoteIfMissing(string repoDir, string repoUrl, bool skipFetch = false);

    /// <summary>
    ///     Checkout the repo to the specified state.
    /// </summary>
    /// <param name="repoDir">Path to a git repository</param>
    /// <param name="commit">Tag, branch, or commit to checkout</param>
    /// <param name="force">Force clean of the repo/submodules</param>
    void Checkout(string repoDir, string commit, bool force = false);

    /// <summary>
    ///     Stages files from the given path.
    /// </summary>
    /// <param name="repoDir">Path to a git repository</param>
    /// <param name="pathToStage">Path that will be staged to index</param>
    void Stage(string repoDir, string pathToStage);

    /// <summary>
    ///     Returns a list of git submodules registered in a given repository.
    /// </summary>
    /// <param name="repoDir">Path to a git repository</param>
    /// <param name="commit">Which commit the info is retrieved for</param>
    List<GitSubmoduleInfo> GetGitSubmodules(string repoDir, string commit);

    /// <summary>
    ///     Returns a list of modified staged files.
    /// </summary>
    /// <param name="repoDir">Path to a git repository</param>
    /// <returns>List of currently modified staged files</returns>
    IEnumerable<string> GetStagedFiles(string repoDir);

    /// <summary>
    /// Pushes a branch to a remote
    /// </summary>
    /// <param name="repoPath">Path of the local repository</param>
    /// <param name="branchName">Name of branch to push</param>
    /// <param name="remoteUrl">URL to push to</param>
    /// <param name="pat">Token for authenticating for pushing</param>
    /// <param name="identity">Identity object containing username and email. Defaults to DarcBot identity</param>
    void Push(
        string repoPath,
        string branchName,
        string remoteUrl,
        string token,
        LibGit2Sharp.Identity identity = null);
}
