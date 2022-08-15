using Microsoft.AspNetCore.Mvc.Filters;

namespace ControlSpark.Core.Infrastructure.Filters
{
    /// <summary>
    /// TrackPagePerformanceFilter 
    /// </summary>
    public class TrackPagePerformanceFilter : IPageFilter
    {
        private readonly ILogger<TrackPagePerformanceFilter> _logger;
        private Stopwatch _timer;

        /// <summary>
        /// TrackPagePerformanceFilter
        /// </summary>
        /// <param name="logger"></param>
        public TrackPagePerformanceFilter(ILogger<TrackPagePerformanceFilter> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// OnPageHandlerExecuting
        /// </summary>
        /// <param name="context"></param>
        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            _timer = new Stopwatch();
            _timer.Start();
        }

        /// <summary>
        /// OnPageHandlerExecuted
        /// </summary>
        /// <param name="context"></param>
        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            _timer.Stop();
            if (context.Exception == null)
            {
                _logger.LogRoutePerformance(context.ActionDescriptor.RelativePath,
                    context.HttpContext.Request.Method,
                    _timer.ElapsedMilliseconds);
            }
            //_logger.LogInformation("{PageName} {Method} model code took {ElapsedMilliseconds}.",
            //    context.ActionDescriptor.RelativePath, 
            //    context.HttpContext.Request.Method, 
            //    _timer.ElapsedMilliseconds);
        }

        /// <summary>
        /// OnPageHandlerSelected
        /// </summary>
        /// <param name="context"></param>
        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
            // not needed
        }
    }
}
