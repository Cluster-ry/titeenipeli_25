using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Titeenipeli.Controllers;
using Titeenipeli.Inputs;
using Titeenipeli.Schema;
using Titeenipeli.Tests.Mocks.Services;
using Xunit;

namespace Titeenipeli.Tests.Controllers;

[TestSubject(typeof(CtfController))]
public class CtfControllerTest
{
    private readonly CtfFlagRepositoryMockService _ctfFlagRepositoryMockService;

    public CtfControllerTest()
    {
        List<CtfFlag> flags =
        [
            new CtfFlag
            {
                Token = "#TEST_FLAG"
            }
        ];

        _ctfFlagRepositoryMockService = new CtfFlagRepositoryMockService(flags);
    }

    [Theory]
    [InlineData("#TEST_FLAG", 200)]
    [InlineData("#INVALID_FLAG", 400)]
    [InlineData(null, 400)]
    public void PostCtfTest(string token, int statusCode)
    {
        CtfController controller = new CtfController(_ctfFlagRepositoryMockService);
        PostCtfInput input = new PostCtfInput
        {
            Token = token
        };

        IStatusCodeActionResult result = controller.PostCtf(input) as IStatusCodeActionResult;

        Assert.Equal(statusCode, result?.StatusCode);
    }
}