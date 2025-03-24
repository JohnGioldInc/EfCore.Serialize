# EFCore.Client Repository

Client library for serializing and deserializing Entity Framework Core queries and saves Remotely.

A Specialized Alternative to OData, GraphQL, and other REST database integrations intended for .NET Stack and Blazor.

A choice of Serializers, You can use either [Remote.Linq](https://github.com/6bee/Remote.Linq) or [Serialize.Linq](https://github.com/esskar/Serialize.Linq)

Without them this would not be possible.

[Code Samples](https://github.com/JohnGoldInc/EfCore.Client/tree/efcore.client-main/samples)

## Blazor Page Usage

To not run affoul of the dreaded 'Cannot wait on monitors on this runtime'

Please use **ToListAsync** and **SaveChangesAsync**

```cs

@code {
    [Inject]
    required public BlazorApp1Context blazorApp1Context { get; set; }

    private List<WeatherForecast>? forecasts;

    protected override async Task OnInitializedAsync()
    {
        forecasts = await blazorApp1Context.WeatherForecast
            .Where(wf => wf.TemperatureC > -999)
            .Include(wf => wf.PrecipitationByHour)
            .OrderBy(wf => wf.Date)
            .Take(5)
            .ToListAsync();
    }
}
```


## Remote.Linq Client Usage
```cs
var builder = WebAssemblyHostBuilder.CreateDefault(args);

var jsonSerializerOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
}.ConfigureRemoteLinq();

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

var expressionTranslator = new ExpressionTranslator();

builder.Services
    .AddDbContext<BlazorApp1Context>(options => options.UseClientDatabase(
        dataProvider,
        changeSaveProvider,
        (expression) => JsonSerializer.Serialize(expressionTranslator.TranslateExpression(expression), jsonSerializerOptions) !));

await builder.Build().RunAsync();
```

## Remote.Linq Server Usage
```cs
    /// <summary>
    /// DataController.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly BlazorApp1Context blazorApp1Context;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataController"/> class.
        /// </summary>
        /// <param name="blazorApp1Context">BlazorApp1Context.</param>
        public DataController(BlazorApp1Context blazorApp1Context)
        {
            this.blazorApp1Context = blazorApp1Context;
            this.jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            }.ConfigureRemoteLinq();
        }

        /// <summary>
        /// Get Data.
        /// </summary>
        /// <param name="serializedExpression">Raw Json string (made possible by RawJsonBodyInputFormatter).</param>
        /// <returns>Data.</returns>
        [HttpPost("query")]
        public ActionResult<IEnumerable<object>> Query([FromBody] string serializedExpression)
        => this.Ok(
            this.blazorApp1Context.FromClientExpression(
                (serializedExpression) => JsonSerializer.Deserialize<Remote.Linq.Expressions.Expression>(serializedExpression, this.jsonSerializerOptions)
                !.ToLinqExpression() !,
                serializedExpression) !);

        /// <summary>
        /// Save Data.
        /// </summary>
        /// <param name="entities">IEnumerable &lt; IUpdateEntry &gt;.</param>
        /// <returns>saved rows.</returns>
        [HttpPost("save")]
        public async ValueTask<ActionResult<int?>> SaveChangesAsync([FromBody] IEnumerable<IUpdateEntry> entities)
            => this.Ok(await this.blazorApp1Context.SaveChangesAsync(entities));
    }
```

## Serialize.Linq Client Usage
```cs
var builder = WebAssemblyHostBuilder.CreateDefault(args);

var jsonSerializerOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
};

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

var serializer = new ExpressionSerializer(new Serialize.Linq.Serializers.JsonSerializer());

builder.Services
    .AddDbContext<BlazorApp1Context>(options => options.UseClientDatabase(
        dataProvider,
        changeSaveProvider,
        (expression) => serializer.SerializeText(expression) !));

await builder.Build().RunAsync();
```

## Serialize.Linq Server Usage
```cs
    /// <summary>
    /// DataController.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly BlazorApp1Context blazorApp1Context;
        private readonly ExpressionSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataController"/> class.
        /// </summary>
        /// <param name="blazorApp1Context">BlazorApp1Context.</param>
        public DataController(BlazorApp1Context blazorApp1Context)
        {
            this.blazorApp1Context = blazorApp1Context;
            this.serializer = new ExpressionSerializer(new JsonSerializer());
        }

        /// <summary>
        /// Get Data.
        /// </summary>
        /// <param name="serializedExpression">Raw Json string (made possible by RawJsonBodyInputFormatter).</param>
        /// <returns>Data.</returns>
        [HttpPost("query")]
        public ActionResult<IEnumerable<object>> Query([FromBody] string serializedExpression)
            => this.Ok(this.blazorApp1Context.FromClientExpression(
                (serializedExpression) => this.serializer.DeserializeText(serializedExpression),
                serializedExpression));

        /// <summary>
        /// Save Data.
        /// </summary>
        /// <param name="entities">IEnumerable &lt; IUpdateEntry &gt;.</param>
        /// <returns>saved rows.</returns>
        [HttpPost("save")]
        public async ValueTask<ActionResult<int?>> SaveChangesAsync([FromBody] IEnumerable<IUpdateEntry> entities)
            => this.Ok(await this.blazorApp1Context.SaveChangesAsync(entities));
    }
```