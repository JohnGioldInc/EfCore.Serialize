// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


// ReSharper disable once CheckNamespace
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Serialize specific extension methods for <see cref="DbContextOptionsBuilder" />.
/// </summary>
public static class SerializeDbContextOptionsExtensions
{
    /// <summary>
    ///     Configures the context to connect to a named Serialize database.
    ///     The Serialize database is shared anywhere the same name is used, but only for a given
    ///     service provider.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-Serialize">The EF Core Serialize database provider</see> for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="databaseName">
    ///     The name of the Serialize database. This allows the scope of the Serialize database to be controlled
    ///     independently of the context. The Serialize database is shared anywhere the same name is used.
    /// </param>
    /// <param name="databaseRoot">
    ///     All Serialize databases will be rooted in this object, allowing the application
    ///     to control their lifetime. This is useful when sometimes the context instance
    ///     is created explicitly with <see langword="new" /> while at other times it is resolved using dependency injection.
    /// </param>
    /// <param name="dataProvider">Your Implementation of getting data over the wire</param>
    /// <param name="saveChangesAsync">Your Implementation for saving over the wire</param>
    /// <param name="serializer"> Serialization Function </param>
    /// <param name="SerializeOptionsAction">An optional action to allow additional Serialize specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseSerializeDatabase(
    this DbContextOptionsBuilder optionsBuilder,
    Func<string, CancellationToken, Task<string>> dataProvider,
    Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> saveChangesAsync,
    Func<Expression, string> serializer,
    string? databaseName = null,
    InMemoryDatabaseRoot? databaseRoot = null,
    Action<DbContextOptionsBuilder>? SerializeOptionsAction = null)
    {
        optionsBuilder.UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString(), databaseRoot);
        var sc = new ServiceCollection().AddEntityFrameworkSerializeDatabase(dataProvider, saveChangesAsync, serializer);
        var sp = sc.BuildServiceProvider(validateScopes: true);
        optionsBuilder.UseInternalServiceProvider(sp);

        if (SerializeOptionsAction != null)
        {
            SerializeOptionsAction.Invoke(optionsBuilder);
        }
        return optionsBuilder;
    }

}
