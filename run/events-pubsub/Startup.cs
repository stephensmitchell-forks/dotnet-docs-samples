// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// [START run_events_pubsub_handler]

using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapPost("/", async context =>
            {
                using (var reader = new StreamReader(context.Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    dynamic cloudEventData = JsonConvert.DeserializeObject(body);
                    if (cloudEventData == null)
                    {
                            context.Response.StatusCode = 400;
                            await context.Response.WriteAsync("Bad request: No Pub/Sub message received");
                            return;
                    }

                    dynamic pubSubMessage = cloudEventData["message"];
                    if (pubSubMessage == null)
                    {
                            context.Response.StatusCode = 400;
                            await context.Response.WriteAsync("Bad request: Invalid Pub/Sub message format");
                            return;
                    }

                    var data = (string)pubSubMessage["data"];
                    var name = Encoding.UTF8.GetString(Convert.FromBase64String(data));
                    var id = context.Request.Headers["ce-id"];
                    await context.Response.WriteAsync($"Hello {name}! ID: {id}");
                }
            });
        });
    }
}
// [END run_events_pubsub_handler]
