using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Maestro.Data;
using Maestro.Web.Api.v2020_02_20.Controllers;
using Maestro.Web.Api.v2020_02_20.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.DarcLib;
using Microsoft.DotNet.Internal.Testing.DependencyInjection.Abstractions;
using Microsoft.DotNet.Internal.Testing.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Commit = Maestro.Web.Api.v2020_02_20.Models.Commit;

namespace Maestro.Web.Tests;

[TestFixture]
public partial class BuildController20200914Tests
{
    static string repository = "FAKE-REPOSITORY";
    static string commitHash = "FAKE-COMMIT";
    static string commitMessage = "FAKE-COMMIT-MESSAGE";
    static string account = "FAKE-ACCOUNT";
    static string project = "FAKE-PROJECT";
    static string branch = "FAKE-BRANCH";
    string buildNumber = "20.9.18.20";

    [Test]
    public async Task CommitIsFound()
    {
        using TestData data = await TestData.Default.BuildAsync();

        int id;
        {
            IActionResult result = await data.Controller.Create(new BuildData
            {
                Commit = commitHash,
                AzureDevOpsAccount = account,
                AzureDevOpsProject = project,
                AzureDevOpsRepository = repository,
                AzureDevOpsBuildNumber = buildNumber,
                AzureDevOpsBranch = branch,
                GitHubBranch = branch,
            });

            result.Should().BeAssignableTo<ObjectResult>();
            var objResult = (ObjectResult)result;
            objResult.StatusCode.Should().Be((int)HttpStatusCode.Created);
            objResult.Value.Should().BeAssignableTo<Build>();
            var build = (Build)objResult.Value;

            id = build.Id;
            build.Commit.Should().Be(commitHash);
            build.AzureDevOpsAccount.Should().Be(account);
            build.AzureDevOpsProject.Should().Be(project);
            build.AzureDevOpsBuildNumber.Should().Be(buildNumber);
            build.AzureDevOpsRepository.Should().Be(repository);
            build.AzureDevOpsBranch.Should().Be(branch);
        }

        {
            var resultCommit = await data.Controller.GetCommit(id);
            var objResultCommit = (ObjectResult)resultCommit;
            objResultCommit.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objResultCommit.Value.Should().BeAssignableTo<Commit>();
            var commit = (Commit)objResultCommit.Value;

            commit.Message.Should().Be(commitMessage);
            commit.Sha.Should().Be(commitHash);
            commit.Author.Should().Be(account);
        }
    }

    [TestDependencyInjectionSetup]
    private static class TestDataConfiguration
    {
        public static async Task Dependencies(IServiceCollection collection)
        {
            string connectionString = await SharedData.Database.GetConnectionString();
            collection.AddLogging(l => l.AddProvider(new NUnitLogger()));

            var mockIRemoteFactory = new Mock<IRemoteFactory>();
            var mockIRemote = new Mock<IRemote>();
            mockIRemoteFactory.Setup(f => f.GetRemoteAsync(repository, It.IsAny<ILogger>())).Returns(Task.FromResult(mockIRemote.Object));
            mockIRemote.Setup(f => f.GetCommitAsync(repository, commitHash)).Returns(Task.FromResult(new Microsoft.DotNet.DarcLib.Commit(account, commitHash, commitMessage)));

            collection.AddSingleton(mockIRemote.Object);
            collection.AddSingleton(mockIRemoteFactory.Object);
            collection.AddSingleton(Mock.Of<IBackgroundQueue>());
            collection.AddSingleton<IHostEnvironment>(new HostingEnvironment
            {
                EnvironmentName = Environments.Development
            });
            collection.AddBuildAssetRegistry(options =>
            {
                options.UseSqlServer(connectionString);
                options.EnableServiceProviderCaching(false);
            });
            collection.AddSingleton<ISystemClock, TestClock>();
        }

        public static Func<IServiceProvider, BuildsController> Controller(IServiceCollection collection)
        {
            collection.AddTransient<BuildsController>();
            return s => s.GetRequiredService<BuildsController>();
        }
    }
}
