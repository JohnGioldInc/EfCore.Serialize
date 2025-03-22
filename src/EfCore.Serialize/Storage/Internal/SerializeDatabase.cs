// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace JohnGoldInc.EntityFrameworkCore.Serialize.Storage.Internal
{
    /// <summary>
    /// Represents a database that supports serialization.
    /// </summary>
    public class SerializeDatabase : IDatabase
    {
        private readonly Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> saveChangesAsync;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializeDatabase"/> class.
        /// </summary>
        public SerializeDatabase(Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> saveChangesAsync)
            => this.saveChangesAsync = saveChangesAsync;

        /// <inheritdoc />
        public Func<QueryContext, TResult> CompileQuery<TResult>(Expression query, bool async)
             => throw new NotImplementedException();

        /// <inheritdoc />
        public Expression<Func<QueryContext, TResult>> CompileQueryExpression<TResult>(Expression query, bool async, IReadOnlySet<string>? nonNullableReferenceTypeParameters = default)
            => throw new NotImplementedException();

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="entries">The entries to save.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public int SaveChanges(IList<IUpdateEntry> entries)
            => SaveChangesAsync(entries).GetAwaiter().GetResult();

        /// <summary>
        /// Saves the changes asynchronously.
        /// </summary>
        /// <param name="entries">The entries to save.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        public Task<int> SaveChangesAsync(IList<IUpdateEntry> entries, CancellationToken cancellationToken = default)
            => this.saveChangesAsync(entries, cancellationToken);

    }
}
