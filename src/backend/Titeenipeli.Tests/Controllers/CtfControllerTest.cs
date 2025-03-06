using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using NUnit.Framework;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.Controllers;
using Titeenipeli.Inputs;
using Titeenipeli.Services;
using Titeenipeli.Services.Grpc;

namespace Titeenipeli.Tests.Controllers;

[TestSubject(typeof(CtfController))]
public class CtfControllerTest
{
    private static readonly Guild OwnGuild = new()
    {
        Name = GuildName.Cluster,
        ActiveCtfFlags = []
    };

    private static readonly User CurrentUser = new()
    {
        Id = 1,
        Guild = OwnGuild,
        Code = "",
        SpawnX = 0,
        SpawnY = 0,
        PowerUps = [],
        TelegramId = "",
        FirstName = "Own user",
        LastName = "",
        Username = ""
    };

    private static readonly JwtClaim CurrentClaim = new()
    {
        Id = 0,
        Guild = OwnGuild.Name,
        CoordinateOffset = new Coordinate(0, 0)
    };


    [TestCase("#TEST_FLAG", 200, TestName = "Should return success code for valid flag")]
    [TestCase("#INVALID_FLAG", 400, TestName = "Should return failure code for invalid flag")]
    [TestCase(null, 400, TestName = "Should return failure code for null flag")]
    public void PostCtfTest(string token, int statusCode)
    {
        var mockCtfFlagRepositoryService = new Mock<ICtfFlagRepositoryService>();
        var mockUserRepositoryService = new Mock<IUserRepositoryService>();
        var mockGuildRepositoryService = new Mock<IGuildRepositoryService>();
        var mockJwtService = new Mock<IJwtService>();
        var mockMiscGameStateUpdateCoreService = new Mock<IMiscGameStateUpdateCoreService>();

        //Setup jwt
        const string jwtClaimName = "JwtClaim";
        mockJwtService
            .Setup(service => service.GetJwtClaimName())
            .Returns(jwtClaimName);

        mockCtfFlagRepositoryService
            .Setup(repositoryService => repositoryService.GetByToken("#TEST_FLAG"))
            .Returns(new CtfFlag { Token = "#TEST_FLAG" });

        mockGuildRepositoryService
        .Setup(repo => repo.Update(It.IsAny<Guild>()));

        mockUserRepositoryService
        .Setup(repo => repo.GetById(It.IsAny<int>()))
        .Returns(CurrentUser);

        var httpContext = new DefaultHttpContext
        {
            Items =
            {
                [jwtClaimName] = CurrentClaim
            }
        };

        var controller = new CtfController(mockCtfFlagRepositoryService.Object, mockUserRepositoryService.Object,
            mockGuildRepositoryService.Object, mockJwtService.Object, mockMiscGameStateUpdateCoreService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        var input = new PostCtfInput
        {
            Token = token
        };

        // ReSharper disable once SuspiciousTypeConversion.Global
        var result = controller.PostCtf(input) as IStatusCodeActionResult;

        result?.StatusCode.Should().Be(statusCode);
    }
}