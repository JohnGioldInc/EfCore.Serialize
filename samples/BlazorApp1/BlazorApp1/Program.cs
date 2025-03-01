// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable SA1200 // Using directives should be placed correctly
using BlazorApp1.Components;
using BlazorApp1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
#pragma warning restore SA1200 // Using directives should be placed correctly

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddDbContext<BlazorApp1Context>(options => options.UseInMemoryDatabase(nameof(BlazorApp1Context)))
    .AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMvc(options =>
{
    options.InputFormatters.Insert(0, new RawJsonBodyInputFormatter());
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlazorApp1 API", Version = "v1" });
});

var app = builder.Build();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "My API V1");
});

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BlazorApp1Context>();

    var startDate = DateOnly.FromDateTime(DateTime.Now);
    var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
    context.AddRange(Enumerable.Range(1, 5).Select(index => new WeatherForecast
    {
        Date = startDate.AddDays(index),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = summaries[Random.Shared.Next(summaries.Length)],
        PrecipitationByHour = Enumerable.Range(0, 23).Select(hour => new PrecipitationByHour
         {
             Date = startDate.AddDays(index),
             Hour = hour,
             Chance = Random.Shared.Next(0, 100),
         }).ToList(),
    }));
    context.SaveChanges();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorApp1.Client._Imports).Assembly);

app.Run();
