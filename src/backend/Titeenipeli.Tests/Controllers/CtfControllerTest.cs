using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using NUnit.Framework;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Controllers;
using Titeenipeli.Inputs;
using Titeenipeli.Services;

namespace Titeenipeli.Tests.Controllers;

[TestSubject(typeof(CtfController))]
public class CtfControllerTest
{
    [TestCase("#TEST_FLAG", 200, TestName = "Should return success code for valid flag")]
    [TestCase("#INVALID_FLAG", 400, TestName = "Should return failure code for invalid flag")]
    [TestCase(null, 400, TestName = "Should return failure code for null flag")]
    public void PostCtfTest(string token, int statusCode)
    {
        Mock<ICtfFlagRepositoryService> mockCtfFlagRepositoryService = new Mock<ICtfFlagRepositoryService>();
        Mock<IUserRepositoryService> mockUserRepositoryService = new Mock<IUserRepositoryService>();
        Mock<IGuildRepositoryService> mockGuildRepositoryService = new Mock<IGuildRepositoryService>();
        Mock<JwtService> mockJwtService = new Mock<JwtService>();

        mockCtfFlagRepositoryService
            .Setup(repositoryService => repositoryService.GetByToken("#TEST_FLAG"))
            .Returns(new CtfFlag { Token = "#TEST_FLAG" });

        CtfController controller = new CtfController(mockCtfFlagRepositoryService.Object, mockUserRepositoryService.Object, mockGuildRepositoryService.Object, mockJwtService.Object);
        PostCtfInput input = new PostCtfInput
        {
            Token = token
        };

        IStatusCodeActionResult result = controller.PostCtf(input) as IStatusCodeActionResult;

        result?.StatusCode.Should().Be(statusCode);
    }
}