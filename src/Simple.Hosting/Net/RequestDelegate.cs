using System.Threading.Tasks;

namespace Simple.Hosting.Net;

public delegate Task RequestDelegate<TContext>(TContext context);