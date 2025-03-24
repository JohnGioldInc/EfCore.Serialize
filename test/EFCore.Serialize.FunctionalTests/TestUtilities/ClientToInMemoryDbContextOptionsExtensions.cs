// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable enable

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SerializeToInMemory specific extension methods for <see cref="DbContextOptionsBuilder" />.
/// </summary>
public static class ClientToInMemoryDbContextOptionsExtensions
{
    /// <summary>
    ///     Configures the context to connect to a named SerializeToInMemory database.
    ///     The SerializeToInMemory database is shared anywhere the same name is used, but only for a given
    ///     service provider.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-SerializeToInMemory">The EF Core SerializeToInMemory database provider</see> for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="databaseName">
    ///     The name of the SerializeToInMemory database. This allows the scope of the SerializeToInMemory database to be controlled
    ///     independently of the context. The SerializeToInMemory database is shared anywhere the same name is used.
    /// </param>
    /// <param name="databaseRoot">
    ///     All SerializeToInMemory databases will be rooted in this object, allowing the application
    ///     to control their lifetime. This is useful when sometimes the context instance
    ///     is created explicitly with <see langword="new" /> while at other times it is resolved using dependency injection.
    /// </param>
    /// <param name="inMemoryOptionsAction">An optional action to allow additional SerializeToInMemory specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseSerializeToInMemoryDatabase<TDbContext>(
        this DbContextOptionsBuilder optionsBuilder,
        string? databaseName = null,
        InMemoryDatabaseRoot? databaseRoot = null,
        Action<InMemoryDbContextOptionsBuilder>? inMemoryOptionsAction = null)
        where TDbContext : DbContext
    {
        var inMemoryOptionsBuilder = new DbContextOptionsBuilder(optionsBuilder.Options);

        inMemoryOptionsBuilder.UseInMemoryDatabase(databaseName ?? typeof(TDbContext).Name, databaseRoot, inMemoryOptionsAction);

        var inMemoryDbContext = (TDbContext)Activator.CreateInstance(typeof(TDbContext), inMemoryOptionsBuilder.Options)!;

        Func<string, CancellationToken, Task<string>> dataProvider = (jsonExpression, cancellationToken)
            => Task.FromResult(
                Newtonsoft.Json.JsonConvert.SerializeObject(
                    inMemoryDbContext.FromSerializedExpression(jsonExpression)
                    )
                );

        Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> changeSaveProvider = (entries, cancellationToken)
            => inMemoryDbContext.SaveChangesAsync(entries, cancellationToken);

        optionsBuilder.UseSerializeDatabase(dataProvider, changeSaveProvider);

        return optionsBuilder;
    }


}
