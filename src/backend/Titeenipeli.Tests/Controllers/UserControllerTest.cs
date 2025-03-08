using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Hosting.Internal;
using Moq;
using NUnit.Framework;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Inputs;
using Titeenipeli.Controllers;
using Titeenipeli.Options;
using Titeenipeli.Services;

namespace Titeenipeli.Tests.Controllers;

[TestFixture]
[TestOf(typeof(UserController))]
public class UserControllerTest
{
    private const string ClaimName = "jwt-claim";

    private readonly JwtService _jwtService = new(new JwtOptions
    {
        ClaimName = ClaimName,
        CookieName = "Auth",
        Encryption = string.Join("", Enumerable.Repeat(0, 32).Select(n => (char)new Random().Next(97, 122))),
        Secret = string.Join("", Enumerable.Repeat(0, 256).Select(n => (char)new Random().Next(97, 122)))
    });

    [TestCase("1", 200, TestName = "Should return success for new user")]
    [TestCase("2", 200, TestName = "Should return success for existing user")]
    public void PostUserTest(string telegramId, int statusCode)
    {
        var botOptions = new BotOptions();
        var gameOptions = new GameOptions();

        var mockUserRepositoryService = new Mock<IUserRepositoryService>();
        mockUserRepositoryService
            .Setup(repositoryService => repositoryService.GetByTelegramId("1"))
            .Returns(null as User);

        mockUserRepositoryService
            .Setup(repositoryService => repositoryService.GetByTelegramId("2"))
            .Returns(new User
            {
                Guild = new Guild { Name = GuildName.Tietokilta },
                Code = "",

                SpawnX = 0,
                SpawnY = 0,

                TelegramId = "",
                FirstName = "",
                LastName = "",
                Username = "",
            });

        var mockGuildRepositoryService = new Mock<IGuildRepositoryService>();

        var mockMapUpdaterService = new Mock<IMapUpdaterService>();
        mockMapUpdaterService.Setup(service => service.PlaceSpawn(mockUserRepositoryService.Object, It.IsAny<User>()))
                             .Returns<IUserRepositoryService, User>((_, user) => new Task<User>(() => user));

        var controller = new UserController(new HostingEnvironment(),
            botOptions,
            gameOptions, mockUserRepositoryService.Object, mockGuildRepositoryService.Object, _jwtService,
            mockMapUpdaterService.Object)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var input = new PostUsersInput
        {
            TelegramId = telegramId,
            Username = "",
            FirstName = "",
            LastName = ""
        };

        // ReSharper disable once SuspiciousTypeConversion.Global
        var result = controller.PostUsers(input) as IStatusCodeActionResult;
        result?.StatusCode.Should().Be(statusCode);
    }
}