using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TemplateProject.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(
            "Processing feature {RequestName}: {@Request}",
            typeof(TRequest).FullName,
            request
        );
        
        _logger.LogTrace("Start processing");
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            var response = await next();
            _logger.LogTrace(
                "Successfully handled. Duration: {Duration}ms",
                Stopwatch.GetElapsedTime(startTime).TotalMilliseconds
            );
            return response;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error while handling. Duration: {Duration}ms",
                Stopwatch.GetElapsedTime(startTime).TotalMilliseconds
            );
            throw;
        }
    }
}