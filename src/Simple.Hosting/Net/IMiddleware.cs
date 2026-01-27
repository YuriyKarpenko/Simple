using System.Threading.Tasks;

namespace Simple.Hosting.Net;

public interface IMiddleware<TContext>
{
    Task InvokeAsync(TContext context, RequestDelegate<TContext> next);
}