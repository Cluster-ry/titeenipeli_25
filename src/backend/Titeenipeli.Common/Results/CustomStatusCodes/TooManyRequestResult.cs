using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Titeenipeli.Common.Results.CustomStatusCodes;

public class TooManyRequestsResult : ObjectResult
{
    private readonly TimeSpan _retryAfter;

    public TooManyRequestsResult([ActionResultObjectValue] object? value, TimeSpan retryAfter = default)
        : base(value)
    {
        StatusCode = StatusCodes.Status429TooManyRequests;
        _retryAfter = retryAfter;
    }

    public override void OnFormatting(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        base.OnFormatting(context);

        IHeaderDictionary responseHeaders = context.HttpContext.Response.Headers;
        if (_retryAfter != default && responseHeaders.RetryAfter.Count == 0)
        {
            responseHeaders.RetryAfter = new RetryConditionHeaderValue(_retryAfter).ToString();
        }
    }
}