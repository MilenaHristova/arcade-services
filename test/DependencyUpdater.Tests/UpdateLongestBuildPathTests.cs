// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maestro.Data.Models;
using Microsoft.DotNet.DarcLib;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace DependencyUpdater.Tests;

[TestFixture]
public class UpdateLongestBuildPathTests : DependencyUpdaterTests
{
    [Test]
    public async Task ShouldSaveCorrectBestAndWorstPathTimesForEachChannel()
    {
        var graph1 = CreateGraph(
            (Repo: "a", BestCaseTime: 1, WorstCaseTime: 7, OnLongestBuildPath: true),
            (Repo: "b", BestCaseTime: 2, WorstCaseTime: 5, OnLongestBuildPath: true),
            (Repo: "c", BestCaseTime: 9, WorstCaseTime: 9, OnLongestBuildPath: false));

        var graph2 = CreateGraph(
            (Repo: "g", BestCaseTime: 10, WorstCaseTime: 70, OnLongestBuildPath: true),
            (Repo: "h", BestCaseTime: 20, WorstCaseTime: 50, OnLongestBuildPath: true));

        SetupBar(
            (ChannelId: 1, Graph: graph1),
            (ChannelId: 2, Graph: graph2));

        var updater = ActivatorUtilities.CreateInstance<DependencyUpdater>(Scope.ServiceProvider);
        await updater.UpdateLongestBuildPathAsync(CancellationToken.None);

        var longestBuildPaths = Context.LongestBuildPaths.ToList();
        longestBuildPaths.Should().HaveCount(2);

        var firstChannelData = longestBuildPaths.FirstOrDefault(x => x.ChannelId == 1);
        firstChannelData.Should().BeEquivalentTo(new LongestBuildPath
        {
            BestCaseTimeInMinutes = 2,
            ChannelId = 1,
            WorstCaseTimeInMinutes = 7,
            ContributingRepositories = "b@main;a@main"
        }, options => options
            .Excluding(x => x.Channel)
            .Excluding(x => x.Id)
            .Excluding(x => x.ReportDate));

        var secondChannelData = longestBuildPaths.FirstOrDefault(x => x.ChannelId == 2);
        secondChannelData.Should().BeEquivalentTo(new LongestBuildPath
        {
            BestCaseTimeInMinutes = 20,
            ChannelId = 2,
            WorstCaseTimeInMinutes = 70,
            ContributingRepositories = "h@main;g@main"
        }, options => options
            .Excluding(x => x.Channel)
            .Excluding(x => x.Id)
            .Excluding(x => x.ReportDate));
    }

    [Test]
    public async Task ShouldNotAddLongestBuildPathRowWhenThereAreNoNodesOnLongestBuildPath()
    {
        var graph = CreateGraph(
            (Repo: "a", BestCaseTime: 1, WorstCaseTime: 7, OnLongestBuildPath: false),
            (Repo: "b", BestCaseTime: 2, WorstCaseTime: 5, OnLongestBuildPath: false));

        SetupBar((ChannelId: 1, Graph: graph));

        var updater = ActivatorUtilities.CreateInstance<DependencyUpdater>(Scope.ServiceProvider);
        await updater.UpdateLongestBuildPathAsync(CancellationToken.None);

        var longestBuildPaths = Context.LongestBuildPaths.ToList();
        longestBuildPaths.Should().BeEmpty();
    }

    [Test]
    public async Task ShouldNotAddLongestBuildPathRowWhenGraphIsEmpty()
    {
        var graph = new DependencyFlowGraph([], []);

        SetupBar((ChannelId: 1, Graph: graph));

        var updater = ActivatorUtilities.CreateInstance<DependencyUpdater>(Scope.ServiceProvider);
        await updater.UpdateLongestBuildPathAsync(CancellationToken.None);

        var longestBuildPaths = Context.LongestBuildPaths.ToList();
        longestBuildPaths.Should().BeEmpty();
    }

    private void SetupBar(
        params (int ChannelId, DependencyFlowGraph Graph)[] graphPerChannel)
    {
        foreach (var item in graphPerChannel)
        {
            BarMock
                .Setup(m => m.GetDependencyFlowGraphAsync(
                    item.ChannelId,
                    It.IsAny<int>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<IReadOnlyList<string>>()))
                .ReturnsAsync(item.Graph);

            Context.Channels.Add(new Channel
            {
                Id = item.ChannelId,
                Name = $"Channel_{item.ChannelId}",
                Classification = "Pizza",
            });
        }

        Context.SaveChanges();
    }

    private static DependencyFlowGraph CreateGraph(
        params (string Repo, double BestCaseTime, double WorstCaseTime, bool OnLongestBuildPath)[] nodes)
    {
        var graphNodes = nodes
            .Select((n, i) => new DependencyFlowNode(n.Repo, "main", $"Node{i}")
            {
                BestCasePathTime = n.BestCaseTime,
                OnLongestBuildPath = n.OnLongestBuildPath,
                WorstCasePathTime = n.WorstCaseTime
            })
            .ToList();

        return new DependencyFlowGraph(graphNodes, []);
    }
}
