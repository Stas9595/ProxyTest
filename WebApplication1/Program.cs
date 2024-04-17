using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using WebApplication1.Middleware;

var builder = WebApplication.CreateBuilder(args);

var httpClient = new HttpClient();

var app = builder.Build();

app.UseMiddleware<ProxyMiddleware>();

app.Run();