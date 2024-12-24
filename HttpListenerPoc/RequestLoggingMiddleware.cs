using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace HttpListenerPoc
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TelemetryClient _telemetryClient;

        public RequestLoggingMiddleware(RequestDelegate next, TelemetryClient telemetryClient)
        {
            _next = next;
            _telemetryClient = telemetryClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {         
            var request = context.Request;
            var requestBody = string.Empty;

            if (request.ContentLength > 0)
            {
                context.Request.EnableBuffering();
                using (var reader = new StreamReader(context.Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Seek(0, SeekOrigin.Begin);
                }
            }
           
            var telemetry = new RequestTelemetry
            {
                Name = $"{request.Method} {request.Path}",
                Timestamp = DateTime.UtcNow,
                ResponseCode = "200",
                Success = true,
            };

            telemetry.Properties["RequestBody"] = requestBody;
            telemetry.Properties["UserAgent"] = request.Headers["User-Agent"];
            telemetry.Properties["PocHttpRequest"] = "TestingMiddleware"; 

            
            _telemetryClient.TrackRequest(telemetry);
           
            await _next(context);
        }
    }
}
