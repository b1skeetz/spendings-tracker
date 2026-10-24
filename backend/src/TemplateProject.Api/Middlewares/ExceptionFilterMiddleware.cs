using JetBrains.Annotations;

namespace TemplateProject.Api.Middlewares;

public class ExceptionFilterMiddleware
{
    private readonly RequestDelegate _next;
    
    private readonly  ILogger<ExceptionFilterMiddleware> _logger;

    public ExceptionFilterMiddleware(RequestDelegate next, ILogger<ExceptionFilterMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    [UsedImplicitly]
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception occured");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.Body = Stream.Null;
        }
    }
}