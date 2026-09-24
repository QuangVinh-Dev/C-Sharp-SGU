using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using UM.Core.Config;

namespace UM.Core.Extensions
{
    public static class RateLimitingExtensions
    {
         public static IServiceCollection AddAppRateLimiting(
        this IServiceCollection services, IConfiguration configuration)
        {
            //Doc section tu appsetting.json 
            var section=configuration.GetSection(RateLimitingOptions.SectionName);
            //Blind json vao RatelimitingOption
            var option=section.Get<RateLimitingOptions>() ?? new();
            //Dang ky dich vu
            services.AddRateLimiter(limiter =>
            {
                limiter.RejectionStatusCode=StatusCodes.Status429TooManyRequests;
                //Dang ky policy tu  config
                foreach (var (name,policy) in option.Policies)
                {
                    limiter.AddFixedWindowLimiter(name,config=>
                    {
                        config.PermitLimit = policy.PermitLimit;
                        config.Window = TimeSpan.FromSeconds(policy.WindowSeconds);
                        config.QueueLimit = policy.QueueLimit;
                        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    });
                }

                //Tuy chinh repose khi bi chan
     
                limiter.OnRejected = async (context, cancellationToken) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)retryAfter.TotalSeconds).ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                    }

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        error = "TooManyRequests",
                        message = "Bạn đã gửi quá nhiều yêu cầu. Vui lòng thử lại sau."
                    }, cancellationToken);
                };
            });

            return services;
        }
    }
}