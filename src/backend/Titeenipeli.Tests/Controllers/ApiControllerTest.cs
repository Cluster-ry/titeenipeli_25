using JetBrains.Annotations;
using Titeenipeli.Controllers;
using Xunit;

namespace Titeenipeli.Tests.Controllers;

[TestSubject(typeof(ApiController))]
public class ApiControllerTest
{
    [Fact]
    public void TestGet()
    {
        var api = new ApiController();
        Assert.True(api.Get() == "pong", "Ping should return pong");
    }
}