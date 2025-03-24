// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Linq.Expressions;
using JohnGoldInc.EntityFrameworkCore.Serialize.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Serialize specific extension methods for <see cref="IServiceCollection" />.
/// </summary>
public static class SerializeServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the services required by the Serialize database provider for Entity Framework
    ///     to an <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     Calling this method is no longer necessary when building most applications, including those that
    ///     use dependency injection in ASP.NET or elsewhere.
    ///     It is only needed when building the internal service provider for use with
    ///     This is not recommend other than for some advanced scenarios.
    /// </remarks>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="dataProvider">Your Implementation of getting data over the wire</param>
    /// <param name="saveChangesAsync">Your Implementation for saving over the wire</param>
    /// <param name="serializer">Your Implementation for serializing expressions</param>
    /// <returns>
    ///     The same service collection so that multiple calls can be chained.
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IServiceCollection AddEntityFrameworkSerializeDatabase(this IServiceCollection serviceCollection, Func<string, CancellationToken, Task<string>> dataProvider, Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> saveChangesAsync, Func<Expression, string> serializer)
    {
        serviceCollection.AddEntityFrameworkInMemoryDatabase();
        serviceCollection.RemoveAll<IDatabase>();
        serviceCollection.TryAddScoped<IDatabase>((sp) => new SerializeDatabase(dataProvider, saveChangesAsync, serializer, sp.GetRequiredService<DatabaseDependencies>()));

        return serviceCollection;
    }
}
