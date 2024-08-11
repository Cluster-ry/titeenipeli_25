using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using NUnit.Framework;
using Titeenipeli.Controllers;
using Titeenipeli.Inputs;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Schema;
using Titeenipeli.Services;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Tests.Controllers;

[TestFixture]
[TestOf(typeof(UserController))]
public class UserControllerTest
{
    private const string ClaimName = "jwt-claim";

    private readonly JwtService _jwtService = new JwtService(new JwtOptions
        {
            ClaimName = ClaimName,
            CookieName = "Auth",
            Encryption = string.Join("", Enumerable.Repeat(0, 32).Select(n => (char)new Random().Next(97, 122))),
            Secret = string.Join("", Enumerable.Repeat(0, 256).Select(n => (char)new Random().Next(97, 122)))
        }
    );

    [TestCase("1", 200, TestName = "Should return success for new user")]
    [TestCase("2", 200, TestName = "Should return success for existing user")]
    public void PostUserTest(string telegramId, int statusCode)
    {
        Mock<IUserRepositoryService> mockUserRepositoryService = new Mock<IUserRepositoryService>();
        mockUserRepositoryService
            .Setup(repositoryService => repositoryService.GetByTelegramId("1"))
            .Returns(new User
                {
                    Guild = null,
                    Code = "",

                    SpawnX = 0,
                    SpawnY = 0,

                    TelegramId = "",
                    FirstName = "",
                    LastName = "",
                    Username = "",
                    PhotoUrl = "",
                    AuthDate = "",
                    Hash = ""
                }
            );

        mockUserRepositoryService
            .Setup(repositoryService => repositoryService.GetByTelegramId("2"))
            .Returns(null as User);

        Mock<IGuildRepositoryService> mockGuildRepositoryService = new Mock<IGuildRepositoryService>();

        UserController controller = new UserController(_jwtService,
            mockUserRepositoryService.Object,
            mockGuildRepositoryService.Object)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        PostUsersInput input = new PostUsersInput
        {
            Id = telegramId,
            AuthDate = "",
            Hash = "",
            Username = "",
            FirstName = "",
            LastName = "",
            PhotoUrl = ""
        };

        IStatusCodeActionResult result = controller.PostUsers(input) as IStatusCodeActionResult;
        result?.StatusCode.Should().Be(statusCode);
    }

    [TestCase(1, "1", 200, TestName = "Should return success with valid guild")]
    [TestCase(1, "2", 400, TestName = "Should return failure with invalid guild")]
    [TestCase(1, null, 400, TestName = "Should return failure without guild")]
    [TestCase(2, "1", 400, TestName = "Should return failure if user already has a guild")]
    public void PutUserTest(int userId, string guild, int statusCode)
    {
        Mock<IUserRepositoryService> mockUserRepositoryService = new Mock<IUserRepositoryService>();
        mockUserRepositoryService
            .Setup(repositoryService => repositoryService.GetById(1))
            .Returns(new User
                {
                    Guild = null,
                    Code = "",

                    SpawnX = 0,
                    SpawnY = 0,

                    TelegramId = "",
                    FirstName = "",
                    LastName = "",
                    Username = "",
                    PhotoUrl = "",
                    AuthDate = "",
                    Hash = ""
                }
            );

        mockUserRepositoryService
            .Setup(repositoryService => repositoryService.GetById(2))
            .Returns(new User
                {
                    Guild = new Guild { Color = 1 },
                    Code = "",

                    SpawnX = 0,
                    SpawnY = 0,

                    TelegramId = "",
                    FirstName = "",
                    LastName = "",
                    Username = "",
                    PhotoUrl = "",
                    AuthDate = "",
                    Hash = ""
                }
            );

        Mock<IGuildRepositoryService> mockGuildRepositoryService = new Mock<IGuildRepositoryService>();
        mockGuildRepositoryService
            .Setup(repositoryService => repositoryService.GetByColor(1))
            .Returns(new Guild { Color = 1 });

        mockGuildRepositoryService
            .Setup(repositoryService => repositoryService.GetByColor(2))
            .Returns(null as Guild);

        UserController controller =
            new UserController(_jwtService,
                mockUserRepositoryService.Object,
                mockGuildRepositoryService.Object)
            {
                ControllerContext =
                {
                    HttpContext = new DefaultHttpContext
                    {
                        Items = new Dictionary<object, object>
                        {
                            {
                                ClaimName,
                                new JwtClaim
                                {
                                    CoordinateOffset = new Coordinate { X = 0, Y = 0 },
                                    GuildId = null,
                                    Id = userId
                                }
                            }
                        }
                    }
                }
            };

        PutUsersInput input = new PutUsersInput { Guild = guild };
        IStatusCodeActionResult result = controller.PutUsers(input) as IStatusCodeActionResult;
        result?.StatusCode.Should().Be(statusCode);
    }
}