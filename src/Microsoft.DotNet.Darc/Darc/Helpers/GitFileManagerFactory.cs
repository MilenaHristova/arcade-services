// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.DotNet.DarcLib;
using Microsoft.DotNet.DarcLib.Helpers;
using Microsoft.DotNet.DarcLib.VirtualMonoRepo;
using Microsoft.Extensions.Logging;

#nullable enable
namespace Microsoft.DotNet.Darc.Helpers;

public class GitFileManagerFactory : IGitFileManagerFactory
{
    private readonly IVmrInfo _vmrInfo;
    private readonly RemoteConfiguration _remoteConfiguration;
    private readonly IProcessManager _processManager;
    private readonly IVersionDetailsParser _versionDetailsParser;
    private readonly ILoggerFactory _loggerFactory;

    public GitFileManagerFactory(
        IVmrInfo vmrInfo,
        RemoteConfiguration remoteConfiguration,
        IProcessManager processManager,
        IVersionDetailsParser versionDetailsParser,
        ILoggerFactory loggerFactory)
    {
        _vmrInfo = vmrInfo;
        _remoteConfiguration = remoteConfiguration;
        _processManager = processManager;
        _versionDetailsParser = versionDetailsParser;
        _loggerFactory = loggerFactory;
    }

    public IGitFileManager Create(string repoUri)
        => new GitFileManager(CreateGitRepo(repoUri), _versionDetailsParser, _loggerFactory.CreateLogger<GitFileManager>());

    private IGitRepo CreateGitRepo(string repoUri) => GitRepoTypeParser.ParseFromUri(repoUri) switch
    {
        GitRepoType.AzureDevOps => new AzureDevOpsClient(
            _processManager.GitExecutable,
            _remoteConfiguration.AzureDevOpsToken,
            _loggerFactory.CreateLogger<AzureDevOpsClient>(),
            _vmrInfo.TmpPath),

        GitRepoType.GitHub => new GitHubClient(
            _processManager.GitExecutable,
            _remoteConfiguration.GitHubToken,
            _loggerFactory.CreateLogger<GitHubClient>(),
            _vmrInfo.TmpPath,
            // Caching not in use for Darc local client.
            null),

        GitRepoType.Local => new LocalGitClient(_remoteConfiguration, _processManager, _loggerFactory.CreateLogger<LocalGitClient>()),
        _ => throw new ArgumentException("Unknown git repository type", nameof(repoUri)),
    };
}
