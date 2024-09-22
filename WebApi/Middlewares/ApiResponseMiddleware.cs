using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebApi.Middlewares
{
    public class ApiResponseMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                context.Response.Body = originalBodyStream;

                if (context.Response.StatusCode == (int)HttpStatusCode.OK && context.Response.ContentType == "application/json")
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var body = await new StreamReader(responseBody).ReadToEndAsync();
                    var formattedResponse = new ApiResponse<object>(JsonConvert.DeserializeObject<object>(body));
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(formattedResponse));
                }
            }
        }
    }

    

}
