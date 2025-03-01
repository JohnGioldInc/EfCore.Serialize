// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable SA1200 // Using directives should be placed correctly
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BlazorApp1.Data;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using Serialize.Linq.Serializers;
#pragma warning restore SA1200 // Using directives should be placed correctly

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var jsonSerializerOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
};

var serializer = new ExpressionSerializer(new Serialize.Linq.Serializers.JsonSerializer());

var httpClient = new HttpClient();

httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);

Func<string, CancellationToken, Task<IEnumerable<object>>> dataProvider = async (json, cancellationToken) =>
{
    var response = await httpClient.PostAsync(new Uri("/api/data/query", UriKind.Relative), new StringContent(json, MediaTypeHeaderValue.Parse("application/json"))).ConfigureAwait(false);

    if (response.StatusCode is HttpStatusCode.InternalServerError)
    {
        byte[] errorMessageData = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        string errorMessage = Encoding.UTF8.GetString(errorMessageData);
        throw new Exception(errorMessage);
    }

    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadFromJsonAsync<IEnumerable<object>>(jsonSerializerOptions).ConfigureAwait(false);
    return result ?? throw new Exception("No result.");
};

Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> changeSaveProvider = async (data, cancellationToken) =>
{
    var response = await httpClient.PostAsJsonAsync(new Uri("/api/data/save", UriKind.Relative), data, jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

    if (response.StatusCode is HttpStatusCode.InternalServerError)
    {
        byte[] errorMessageData = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        string errorMessage = Encoding.UTF8.GetString(errorMessageData);
        throw new Exception(errorMessage);
    }

    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadFromJsonAsync<int?>(jsonSerializerOptions).ConfigureAwait(false);
    return result ?? throw new Exception("Received empty value from server");
};

builder.Services
    .AddDbContext<BlazorApp1Context>(options => options.UseSerializeDatabase(Guid.NewGuid().ToString(), dataProvider, changeSaveProvider));

await builder.Build().RunAsync();
