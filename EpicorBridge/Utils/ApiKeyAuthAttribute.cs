using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EpicorBridge.Utils
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        //Look for "ApiKey" in query params
        private const string ApiKeyHeaderName = "api_key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //Scan incoming HttpContext Query collection for requested key, return value if exists
            if (!context.HttpContext.Request.Query.TryGetValue(ApiKeyHeaderName, out var potentialApiKeyValue))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            //Get configured Value of specified Key from config
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = configuration.GetValue<string>("api_key");

            if (!apiKey.Equals(potentialApiKeyValue))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }
}
