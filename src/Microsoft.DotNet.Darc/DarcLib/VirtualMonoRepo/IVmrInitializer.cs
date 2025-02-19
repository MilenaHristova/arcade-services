// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.DarcLib.Helpers;

#nullable enable
namespace Microsoft.DotNet.DarcLib.VirtualMonoRepo;

public interface IVmrInitializer
{
    /// <summary>
    /// Initializes new repo that hasn't been synchronized into the VMR yet.
    /// </summary>
    /// <param name="mappingName">Name of a repository mapping</param>
    /// <param name="targetRevision">Revision (commit SHA, branch, tag..) onto which to synchronize, leave empty for HEAD</param>
    /// <param name="targetVersion">Version of packages, that the SHA we're updating to, produced</param>
    /// <param name="initializeDependencies">When true, initializes dependencies (from Version.Details.xml) recursively</param>
    /// <param name="sourceMappingsPath">Path to the source-mappings.json file</param>
    /// <param name="additionalRemotes">Additional git remotes to use when fetching</param>
    /// <param name="readmeTemplatePath">Path to VMR's README.md template</param>
    /// <param name="tpnTemplatePath">Path to VMR's THIRD-PARTY-NOTICES.md template</param>
    /// <param name="discardPatches">Whether to clean up genreated .patch files after their used</param>
    Task InitializeRepository(
        string mappingName,
        string? targetRevision,
        string? targetVersion,
        bool initializeDependencies,
        LocalPath sourceMappingsPath,
        IReadOnlyCollection<AdditionalRemote> additionalRemotes,
        string? readmeTemplatePath,
        string? tpnTemplatePath,
        bool discardPatches,
        CancellationToken cancellationToken);
}
