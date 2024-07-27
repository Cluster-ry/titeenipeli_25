using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using Titeenipeli.Controllers;
using Titeenipeli.Inputs;
using Titeenipeli.Schema;
using Titeenipeli.Services.RepositoryServices.Interfaces;
using NUnit.Framework;
using FluentAssertions;

namespace Titeenipeli.Tests.Controllers;

[TestSubject(typeof(CtfController))]
public class CtfControllerTest
{
    [TestCase("#TEST_FLAG", 200, TestName="Should return success code for valid flag")]
    [TestCase("#INVALID_FLAG", 400, TestName="Should return failure code for invalid flag")]
    [TestCase(null, 400, TestName="Should return failure code for null flag")]
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