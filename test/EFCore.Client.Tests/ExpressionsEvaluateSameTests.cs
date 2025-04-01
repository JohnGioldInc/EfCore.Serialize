// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using Newtonsoft.Json;
using Remote.Linq;
using Remote.Linq.Newtonsoft.Json;
using Serialize.Linq.Serializers;

namespace EFCore.Client.Tests
{
    public class ExpressionsEvaluateSameTests
    {
        [Theory]
        [MemberData(nameof(ExpressionsEvaluateSameTestData))]
        public async Task ExpressionsEvaluateSameTest(bool async, ClientTestModeType clientTestMode, Func<IQueryable<WeatherForecast>, IQueryable<WeatherForecast>> expression)
        {
            BlazorApp1Context inMemoryContext;
            Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> saveChangesAsync;
            GetInMemoryContextAndSave(out inMemoryContext, out saveChangesAsync);

            Func<string, Expression> remoteLinqDeserializer = null!;
            var clientContext = GetClientContext(inMemoryContext, saveChangesAsync, clientTestMode, ref remoteLinqDeserializer);

            var expected = async
                ? await expression(inMemoryContext.WeatherForecast).ToListAsync()
                : expression(inMemoryContext.WeatherForecast).ToList();

            var actual = async
                ? await expression(clientContext.WeatherForecast).ToListAsync()
                : expression(clientContext.WeatherForecast).ToList();

            Assert.Equal(expected.Count(), actual.Count());
            Assert.Equal(expected.SelectMany(x=>x.PrecipitationByHour!).Count(), actual.SelectMany(x => x.PrecipitationByHour!).Count());
        }
        public static IEnumerable<object[]> ExpressionsEvaluateSameTestData
        => Enumerable.Range(0, 2).SelectMany(async =>
            Enumerable.Range(1, 1).SelectMany(clientTestMode =>
            new object[][] {
                new object[] { async == 1, (ClientTestModeType)clientTestMode, (Func<IQueryable<WeatherForecast>, IQueryable<WeatherForecast>>)
                    (source => source.Where<WeatherForecast>(x => x.TemperatureC > 20))},
                new object[] { async == 1, (ClientTestModeType)clientTestMode, (Func<IQueryable<WeatherForecast>, IQueryable<WeatherForecast>>)
                    (source => source.Where(x => x.TemperatureC > 20)
                                        .Include(x=>x.PrecipitationByHour)) },
                new object[] { async == 1, (ClientTestModeType)clientTestMode, (Func<IQueryable<WeatherForecast>, IQueryable<WeatherForecast>>)
                    (source => source.Where(x => x.TemperatureC > 20)
                                        .Include(x=>x.PrecipitationByHour)
                                        .OrderBy(x=>x.Date)
                                        .Skip(1)
                                        .Take(2)
                    )},
            }));

        private static BlazorApp1Context GetClientContext(BlazorApp1Context inMemoryContext, Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> saveChangesAsync, ClientTestModeType clientTestMode, ref Func<string, Expression> deserializer)
        {
            Func<System.Linq.Expressions.Expression, string> serializer = null!;
            Func<string, CancellationToken, Task<string>> dataProvider = null!;

            GetSerializerAndDataProviderForInMemoryDbContext(inMemoryContext, clientTestMode, ref serializer, ref dataProvider, ref deserializer);

            var clientOptions = (DbContextOptions<BlazorApp1Context>)new DbContextOptionsBuilder<BlazorApp1Context>()
                .UseClientDatabase(dataProvider, saveChangesAsync, serializer, Guid.NewGuid().ToString())
                .Options;
            var clientContext = new BlazorApp1Context(clientOptions);
            return clientContext;
        }

        private static void GetInMemoryContextAndSave(out BlazorApp1Context inMemoryContext, out Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> saveChangesAsync)
        {
            var inMemoryOptions = new DbContextOptionsBuilder<BlazorApp1Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            inMemoryContext = new BlazorApp1Context(inMemoryOptions);
            var startDate = DateOnly.FromDateTime(DateTime.Now);
            var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
            inMemoryContext.AddRange(Enumerable.Range(1, 14).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)],
                PrecipitationByHour = Enumerable.Range(0, 24).Select(hour => new PrecipitationByHour
                {
                    Date = startDate.AddDays(index),
                    Hour = hour,
                    Chance = Random.Shared.Next(0, 100),
                }).ToList(),
            }));
            inMemoryContext.SaveChanges();


            saveChangesAsync = GetSaveChangesAsync(inMemoryContext);
        }

        private static Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> GetSaveChangesAsync<TContext>(TContext inMemoryDbContext) where TContext : DbContext => (data, cancellationToken) =>
        {
            var serializedEntities = System.Text.Json.JsonSerializer.Serialize(data);
            var deserializedEntities = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<IUpdateEntry>>(serializedEntities)!;

            var result = inMemoryDbContext.SaveChangesAsync(deserializedEntities);

            return result;
        };

        private static void GetSerializerAndDataProviderForInMemoryDbContext<TContext>(TContext inMemoryDbContext, ClientTestModeType clientTestMode, ref Func<System.Linq.Expressions.Expression, string> serializer, ref Func<string, CancellationToken, Task<string>> dataProvider, ref Func<string, Expression> deserializer) where TContext : DbContext
        {
            if (clientTestMode == ClientTestModeType.Remote_Linq)
            {
                JsonSerializerSettings serializerSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore}.ConfigureRemoteLinq();

                serializer = (expression) => JsonConvert.SerializeObject(expression, serializerSettings);

                Func<string, Expression> p_deserializer = (serializedExpression) => JsonConvert.DeserializeObject<Remote.Linq.Expressions.MethodCallExpression>(serializedExpression, serializerSettings)!.ToLinqExpression()!;
                deserializer = p_deserializer;

                dataProvider = (serializedExpression, cancellationToken) =>
                {
                    var result = inMemoryDbContext.FromClientExpression(p_deserializer, serializedExpression)!;

                    return Task<string>.FromResult(JsonConvert.SerializeObject(result, serializerSettings));
                };
            }
            else if (clientTestMode == ClientTestModeType.Serialize_Linq)
            {
                serializer = (expression) => new ExpressionSerializer(new Serialize.Linq.Serializers.JsonSerializer()).SerializeText(expression);

                Func<string, Expression> p_deserializer = (serializedExpression) => new ExpressionSerializer(new Serialize.Linq.Serializers.JsonSerializer()).DeserializeText(serializedExpression);
                deserializer = p_deserializer;

                dataProvider = (serializedExpression, cancellationToken) =>
                {
                    var result = inMemoryDbContext.FromClientExpression(p_deserializer, serializedExpression)!;

                    return Task<string>.FromResult(System.Text.Json.JsonSerializer.Serialize(result));
                };
            }
        }

        /// <summary>
        /// BlazorApp1Context.
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="BlazorApp1Context"/> class.
        /// </remarks>
        /// <param name="options">options.</param>
        public class BlazorApp1Context(DbContextOptions<BlazorApp1Context> options)
            : DbContext(options)
        {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

            /// <summary>
            /// Gets or sets WeatherForecast.
            /// </summary>
            public DbSet<WeatherForecast> WeatherForecast { get; set; }

            /// <summary>
            /// Gets or sets PrecipitationByHour.
            /// </summary>
            public DbSet<PrecipitationByHour> PrecipitationByHour { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        }

#pragma warning disable SA1402 // File may only contain a single type
        /// <summary>
        ///     WeatherForecast.
        /// </summary>
        public class WeatherForecast
        {
            /// <summary>
            /// Gets or sets Date.
            /// </summary>
            [Key]
            public DateOnly Date { get; set; }

            /// <summary>
            /// Gets or sets TemperatureC.
            /// </summary>
            public int TemperatureC { get; set; }

            /// <summary>
            /// Gets or sets Summary.
            /// </summary>
            public string? Summary { get; set; }

            /// <summary>
            /// Gets TemperatureF.
            /// </summary>
            public int TemperatureF => 32 + (int)(this.TemperatureC / 0.5556);

            /// <summary>
            /// Gets or sets PrecipitationByHour.
            /// </summary>
            [ForeignKey("Date")]
            public ICollection<PrecipitationByHour>? PrecipitationByHour { get; set; }
        }

        /// <summary>
        ///    PrecipitationByHour.
        /// </summary>
        [PrimaryKey(nameof(Date), nameof(Hour))]
        public class PrecipitationByHour
        {
            /// <summary>
            /// Gets or sets Date.
            /// </summary>
            public DateOnly Date { get; set; }

            /// <summary>
            /// Gets or sets Hour.
            /// </summary>
            public int Hour { get; set; }

            /// <summary>
            /// Gets or sets Chance.
            /// </summary>
            public int Chance { get; set; }
        }
#pragma warning restore SA1402 // File may only contain a single type
    }


    public enum ClientTestModeType
    {
        Remote_Linq,
        Serialize_Linq
    }

}
