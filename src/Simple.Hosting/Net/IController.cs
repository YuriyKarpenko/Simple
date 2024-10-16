using System.Threading.Tasks;

namespace Simple.Hosting.Net;

public interface IController<TContext>
{
    Task InvokeAsync(TContext context);
    //RequestDelegate<TContext> InvokeAsync;
}
