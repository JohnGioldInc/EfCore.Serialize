# EfCore.Serialize Repository

Client library for serializing and deserializing Entity Framework Core queries and saves Remotely.

Used Sterilize.Linq and EfCore.InMemory to abstract client calls to the server.

Alternative to OData, GraphQL, and other REST database integrations

[Code Samples](https://github.com/JohnGioldInc/EfCore.Serialize/tree/efcore.serialize-main/samples)

## Client Usage
```cs
var httpClient = new HttpClient();

httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);

Func<string, CancellationToken, Task<string>> dataProvider = async (jsonExpression, cancellationToken) =>
{
    var response = await httpClient.PostAsync(new Uri("/api/data/query", UriKind.Relative), new StringContent(jsonExpression, MediaTypeHeaderValue.Parse("application/json"))).ConfigureAwait(false);

    if (response.StatusCode is HttpStatusCode.InternalServerError)
    {
        byte[] errorMessageData = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        string errorMessage = Encoding.UTF8.GetString(errorMessageData);
        throw new Exception(errorMessage);
    }

    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
    .AddDbContext<BlazorApp1Context>(options => options.UseSerializeDatabase(dataProvider, changeSaveProvider));
```

## Server Usage
```cs
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly BlazorApp1Context blazorApp1Context;

        public DataController(BlazorApp1Context blazorApp1Context)
        {
            this.blazorApp1Context = blazorApp1Context;
        }

        [HttpPost("query")]
        public ActionResult<IEnumerable<object>> Query([FromBody] string serializedExpression)
            => this.Ok(this.blazorApp1Context.FromSerializedExpression(serializedExpression));

        [HttpPost("save")]
        public async ValueTask<ActionResult<int?>> SaveChangesAsync([FromBody] IEnumerable<IUpdateEntry> entities)
            => this.Ok(await this.blazorApp1Context.SaveChangesAsync(entities));
    }
```