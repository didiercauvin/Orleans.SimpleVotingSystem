using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVotingSystem.Silo;

public static class HealthCheckExtensions
{
    public static void MapHealthChecksWithJsonResponse(this IEndpointRouteBuilder endpoints, PathString path)
    {
        var options = new HealthCheckOptions
        {
            ResponseWriter = async (httpContext, healthReport) =>
            {
                httpContext.Response.ContentType = "application/json";

                var result = JsonConvert.SerializeObject(new
                {
                    status = healthReport.Status.ToString(),
                    totalDurationInSeconds = healthReport.TotalDuration.TotalSeconds,
                    entries = healthReport.Entries.Select(e => new
                    {
                        key = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        data = e.Value.Data
                    })
                });
                await httpContext.Response.WriteAsync(result);
            }
        };
        endpoints.MapHealthChecks(path, options);
    }
}
