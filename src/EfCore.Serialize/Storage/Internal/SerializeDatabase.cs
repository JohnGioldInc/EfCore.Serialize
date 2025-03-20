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
        private readonly Func<string, CancellationToken, Task<string>> dataProvider;
        private readonly Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> saveChangesAsync;
        private readonly Func<Expression, string> serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializeDatabase"/> class.
        /// </summary>
        public SerializeDatabase(Func<string, CancellationToken, Task<string>> dataProvider, Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> saveChangesAsync, Func<Expression,string> serializer)
        {
            this.dataProvider = dataProvider;
            this.saveChangesAsync = saveChangesAsync;
            this.serializer = serializer;
        }

        /// <summary>
        /// Compiles the specified query.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query expression.</param>
        /// <param name="async">if set to <see langword="true"/> [asynchronous].</param>
        /// <returns>A function that executes the query.</returns>
        public Func<QueryContext, TResult> CompileQuery<TResult>(Expression query, bool async)
            => CompileQueryExpression<TResult>(query, async).Compile();

        /// <summary>
        /// Compiles the query expression.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query expression.</param>
        /// <param name="async">if set to <see langword="true"/> [asynchronous].</param>
        /// <param name="nonNullableReferenceTypeParameters">The non-nullable reference type parameters.</param>
        /// <returns>An expression that represents the compiled query.</returns>
        public Expression<Func<QueryContext, TResult>> CompileQueryExpression<TResult>(Expression query, bool async, IReadOnlySet<string>? nonNullableReferenceTypeParameters = default)
        {
            query = ReplaceEntityQueryRootExpressionVisitor.Instance.Visit(query);

            var serialized = serializer(query);
            var genericType = typeof(TResult).GetGenericArguments()?.FirstOrDefault()?.GetGenericArguments()?.FirstOrDefault()
                ?? typeof(TResult).GetGenericArguments()?.FirstOrDefault()
                ?? typeof(TResult);
            var queryingEnumerableType = typeof(QueryingEnumerable<>).MakeGenericType(genericType);

            var data = dataProvider(serialized, CancellationToken.None);

            var queryContextParameter = Expression.Parameter(typeof(QueryContext), "queryContext");

            var dataParameter = Expression.Constant(data);

            var newQueryingEnumerable = Expression.New(queryingEnumerableType.GetConstructor(new[] { typeof(Task<string>) })!, dataParameter);

            var lambda = Expression.Lambda<Func<QueryContext, TResult>>(newQueryingEnumerable, queryContextParameter);
            return lambda;
        }

        private class ReplaceEntityQueryRootExpressionVisitor : ExpressionVisitor
        {
            public static readonly ReplaceEntityQueryRootExpressionVisitor Instance = _Instance ??= new ReplaceEntityQueryRootExpressionVisitor();
            private static ReplaceEntityQueryRootExpressionVisitor _Instance;

            /// <summary>
            ///     Rewrites <see cref="EntityQueryRootExpression" /> encountered in an expression to use a different entity type.
            /// </summary>
            /// <param name="expression">The query expression to rewrite.</param>
            public Expression Rewrite(Expression expression)
                => Visit(expression);

            /// <inheritdoc />
            protected override Expression VisitExtension(Expression extensionExpression)
                => extensionExpression is EntityQueryRootExpression entityQueryRootExpression
                    ? Expression.Parameter(typeof(IQueryable<>).MakeGenericType(entityQueryRootExpression.EntityType.ClrType), "queryable")
                    : base.VisitExtension(extensionExpression);

            /// <inheritdoc />
            protected override Expression VisitParameter(ParameterExpression parameterExpression)
                => base.VisitParameter(parameterExpression);
        }

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

        private sealed class QueryingEnumerable<T>(Task<string> data)
            : IAsyncEnumerable<T>, IEnumerable<T>
             where T : class
        {
            private QueryingEnumerator<T>? enumerator;

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => enumerator ?? (enumerator = new QueryingEnumerator<T>(data, cancellationToken));

            public IEnumerator<T> GetEnumerator()
                 => enumerator ?? (enumerator = new QueryingEnumerator<T>(data));

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private sealed class QueryingEnumerator<E>(Task<string> data, CancellationToken cancellationToken = default) : IAsyncEnumerator<E>, IEnumerator<E>
                where E : class
            {
                private IEnumerable<E>? enumerable;
                private IEnumerator<E>? enumerableEnumerator;

                public E Current=> (E)enumerableEnumerator!.Current!;

                E IEnumerator<E>.Current => Current;

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                    enumerableEnumerator?.Dispose();
                    enumerableEnumerator = null;
                    enumerable = null;
                }

                public ValueTask DisposeAsync()
                {
                    this.Dispose();
                    return ValueTask.CompletedTask;
                }

                public bool MoveNext()
                    => MoveNextAsync().AsTask().Result;

                public async ValueTask<bool> MoveNextAsync()
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        await DisposeAsync().ConfigureAwait(false);
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    enumerable ??= Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<E>>(await data.ConfigureAwait(false));
                    enumerableEnumerator ??= enumerable!.GetEnumerator();

                    return enumerableEnumerator!.MoveNext();
                }

                public void Reset() => throw new NotImplementedException();
            }
        }
    }
}
