using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WebApplication1.Middleware;

public class ProxyMiddleware
{
    private readonly RequestDelegate _next;
    private static HttpClient _client = new HttpClient();

    public ProxyMiddleware(RequestDelegate next)
    {
        _next = next;
        _client.BaseAddress = new Uri("https://www.lipsum.com/");
    }

    public async Task Invoke(HttpContext context)
    {
        var request = new HttpRequestMessage();
        var path = context.Request.Path.Value.Length > 1 ? context.Request.Path.Value.Substring(1) : "";
        var siteResponse = await _client.GetAsync(path);

        if (!siteResponse.IsSuccessStatusCode)
        {
            context.Response.StatusCode = (int)siteResponse.StatusCode;
            await context.Response.WriteAsync(await siteResponse.Content.ReadAsStringAsync());
            return;
        }

        if (siteResponse.Content.Headers.ContentType.MediaType == "text/html")
        {
            var content = await siteResponse.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            foreach (var textNode in doc.DocumentNode.DescendantsAndSelf().Where(n => n.NodeType == HtmlNodeType.Text))
            {
                textNode.InnerHtml = Regex.Replace(textNode.InnerHtml, @"\b\w{6}\b", "$&™");
            }

            if (_client.BaseAddress is not null)
            {
                foreach (var link in doc.DocumentNode.DescendantsAndSelf().Where(n => n.Name == "a"))
                {
                    var href = link.GetAttributeValue("href", string.Empty);
                    if (!string.IsNullOrWhiteSpace(href) && href.StartsWith("http"))
                    {
                        href = href.Replace(_client.BaseAddress.ToString() ?? string.Empty, "http://localhost:5000"); // Replace "http://originaldomain.com" with the domain that you are proxying.
                        link.SetAttributeValue("href", href);
                    }
                }   
            }
            
            content = doc.DocumentNode.OuterHtml;
            await context.Response.WriteAsync(content);
        }
        else
        {
            await context.Response.Body.WriteAsync(await siteResponse.Content.ReadAsByteArrayAsync());
        }
    }
}