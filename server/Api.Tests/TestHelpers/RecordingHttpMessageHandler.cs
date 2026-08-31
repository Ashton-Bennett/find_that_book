using System.Net;

namespace Api.Tests.TestHelpers;

internal sealed class RecordingHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

    public RecordingHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        _responseFactory = responseFactory;
    }

    public Uri? RequestUri { get; private set; }

    public string? RequestBody { get; private set; }

    public HttpRequestHeadersSnapshot? RequestHeaders { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestUri = request.RequestUri;
        RequestBody = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken);
        RequestHeaders = new HttpRequestHeadersSnapshot(request);

        return _responseFactory(request);
    }
}

internal sealed class HttpRequestHeadersSnapshot
{
    private readonly HttpRequestMessage _request;

    public HttpRequestHeadersSnapshot(HttpRequestMessage request)
    {
        _request = request;
    }

    public string? GetSingleValue(string name) =>
        _request.Headers.TryGetValues(name, out var values)
            ? values.SingleOrDefault()
            : null;
}
