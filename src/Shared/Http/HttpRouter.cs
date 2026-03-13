namespace Shared.Http;

using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Web;
public class HttpRouter
{
	public const int RESPONSE_NOT_SENT = 777;
	private static ulong requestId = 0;
	private string basePath;
	private List<HttpMiddleware> middlewares;
	private List<(string, string, HttpMiddleware[])> routes;
	public HttpRouter()
	{
		basePath = string.Empty;
		middlewares = [];
		routes = [];
	}

	public HttpRouter Use(params HttpMiddleware[] middlewares)
	{
		this.middlewares.AddRange(middlewares);
		return this;
	}
	public HttpRouter Map(string method, string path, params HttpMiddleware[] middlewares)

	{
		routes.Add((method.ToUpperInvariant(), path, middlewares));
		return this;
	}

	public HttpRouter MapGet(string path, params HttpMiddleware[] middlewares)
	{
		return Map("GET", path, middlewares);
	}

	public HttpRouter MapPost(string path, params HttpMiddleware[] middlewares)
	{
		return Map("POST", path, middlewares);
	}

	public HttpRouter MapPut(string path, params HttpMiddleware[] middlewares)
	{
		return Map("PUT", path, middlewares);
	}

	public HttpRouter MapDelete(string path, params HttpMiddleware[] middlewares)
	{
		return Map("DELETE", path, middlewares);
	}

	public async Task HandleContextAsync(HttpListenerContext ctx)
	{
		var req = ctx.Request;
		var res = ctx.Response;
		var props = new Hashtable();
		res.StatusCode = RESPONSE_NOT_SENT;
		props["req.id"] = ++requestId;
		try
		{
			await HandleAsync(req, res, props, () => Task.CompletedTask);
		}
		finally
		{
			if (res.StatusCode == RESPONSE_NOT_SENT)
			{
				res.StatusCode = (int)HttpStatusCode.NotImplemented;
			}
			res.Close();
		}
	}
	private async Task HandleAsync(HttpListenerRequest req,
HttpListenerResponse res, Hashtable props, Func<Task> next)
	{
		Func<Task> globalMiddlewarePipeline =
		GenerateMiddlewarePipeline(req, res, props, middlewares);
		await globalMiddlewarePipeline();
		await next();
	}
	public HttpRouter UseRouter(string path, HttpRouter router)
	{
		router.basePath = this.basePath + path;
		return Use(router.HandleAsync);
	}

}