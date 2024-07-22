using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using Titeenipeli.Controllers;
using Titeenipeli.Inputs;
using Titeenipeli.Schema;
using Titeenipeli.Services.RepositoryServices.Interfaces;
using Xunit;
using FluentAssertions;

namespace Titeenipeli.Tests.Controllers;

[TestSubject(typeof(CtfController))]
public class CtfControllerTest
{
    [Theory]
    [InlineData("#TEST_FLAG", 200)]
    [InlineData("#INVALID_FLAG", 400)]
    [InlineData(null, 400)]
    public void PostCtfTest(string token, int statusCode)
    {
        Mock<ICtfFlagRepositoryService> mockCtfFlagRepositoryService = new Mock<ICtfFlagRepositoryService>();
        mockCtfFlagRepositoryService
            .Setup(repositoryService => repositoryService.GetByToken("#TEST_FLAG"))
            .Returns(new CtfFlag { Token = "#TEST_FLAG" });

        CtfController controller = new CtfController(mockCtfFlagRepositoryService.Object);
        PostCtfInput input = new PostCtfInput
        {
            Token = token
        };

        IStatusCodeActionResult result = controller.PostCtf(input) as IStatusCodeActionResult;

        result?.StatusCode.Should().Be(statusCode);
    }
}