// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace JohnGoldInc.EntityFrameworkCore.Client.Storage.Internal
{
    /// <summary>
    /// Represents a database that supports serialization.
    /// </summary>
    public class ClientDatabase : Database, IDatabase
    {
        private readonly Func<string, CancellationToken, Task<string>> dataProvider;
        private readonly Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> saveChangesAsync;
        private readonly Func<Expression, string> serializer;
        //   private readonly QueryCompilationContext queryCompilationContext;


        /// <summary>
        /// Initializes a new instance of the <see cref="ClientDatabase"/> class.
        /// </summary>
        public ClientDatabase(
            Func<string, CancellationToken, Task<string>> dataProvider,
            Func<IEnumerable<IUpdateEntry>, CancellationToken, Task<int>> saveChangesAsync,
            Func<Expression, string> serializer,
            DatabaseDependencies dependencies) : base(dependencies)
        {
            this.dataProvider = dataProvider;
            this.saveChangesAsync = saveChangesAsync;
            this.serializer = serializer;
            // this.queryCompilationContext  = dependencies.QueryCompilationContextFactory.Create(true);
        }

        /// <summary>
        /// Compiles the specified query.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query expression.</param>
        /// <param name="async">if set to <see langword="true"/> [asynchronous].</param>
        /// <returns>A function that executes the query.</returns>
        public override Func<QueryContext, TResult> CompileQuery<TResult>(Expression query, bool async)
#pragma warning disable EF9100 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            => CompileQueryExpression<TResult>(query, async).Compile();
#pragma warning restore EF9100 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        /// <summary>
        /// Compiles the query expression.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="query">The query expression.</param>
        /// <param name="async">if set to <see langword="true"/> [asynchronous].</param>
        /// <param name="nonNullableReferenceTypeParameters">The non-nullable reference type parameters.</param>
        /// <returns>An expression that represents the compiled query.</returns>
        public override Expression<Func<QueryContext, TResult>> CompileQueryExpression<TResult>(Expression query, bool async, IReadOnlySet<string>? nonNullableReferenceTypeParameters = default)
        {
            var type = typeof(TResult).GetGenericArguments()?.FirstOrDefault()?.GetGenericArguments()?.FirstOrDefault()
                    ?? typeof(TResult).GetGenericArguments()?.FirstOrDefault()
                    ?? typeof(TResult);

            Expression<Func<QueryContext, TResult>> lambda = (queryContext) =>
                 (TResult)typeof(QueryingEnumerable<>)
                 .MakeGenericType(type)
                 .GetConstructor(new[] { typeof(Task<string>) })!
                 .Invoke(new[] {
                         dataProvider(serializer(new ReplaceEntityQueryRootExpressionAndParametersVisitor(queryContext).Visit(query)), CancellationToken.None)
                     });

            return lambda;
        }


        private class ReplaceEntityQueryRootExpressionAndParametersVisitor(QueryContext queryContext) : ExpressionVisitor
        {
            public Expression Rewrite(Expression expression)
                => Visit(expression);

            protected override Expression VisitParameter(ParameterExpression parameterExpression)
                => queryContext.ParameterValues.ContainsKey(parameterExpression.Name!)
                ? Expression.Constant(queryContext.ParameterValues[parameterExpression.Name!])
                : base.VisitParameter(parameterExpression);

            protected override Expression VisitExtension(Expression extensionExpression)
                => extensionExpression is EntityQueryRootExpression entityQueryRootExpression
                ? Expression.Parameter(typeof(IQueryable<>).MakeGenericType(entityQueryRootExpression.EntityType.ClrType), "queryable")
                : base.VisitExtension(extensionExpression);
        }

        private class QueryingEnumerable<T>(Task<string> data)
            : IAsyncEnumerable<T>, IEnumerable<T>, IQueryable<T>, IQueryable, IOrderedQueryable<T>, IOrderedQueryable
             where T : class
        {
            private QueryingEnumerator<T>? enumerator;

            public Type ElementType => typeof(T);

            public Expression Expression => Expression.Parameter(typeof(IQueryable<T>));

            public IQueryProvider Provider => new CloneQueryProvider(this);

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

                public E Current => (E)enumerableEnumerator!.Current!;

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
            private class CloneQueryProvider(IQueryable<T> queriable) : IQueryProvider
            {
                public IQueryable CreateQuery(Expression expression)
                    => queriable;
                IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
                    => (IQueryable<TElement>)queriable;
                public object? Execute(Expression expression) => CreateQuery(expression);
                public TResult Execute<TResult>(Expression expression) => (TResult)CreateQuery(expression);
            }
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="entries">The entries to save.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public override int SaveChanges(IList<IUpdateEntry> entries)
            => SaveChangesAsync(entries).GetAwaiter().GetResult();

        /// <summary>
        /// Saves the changes asynchronously.
        /// </summary>
        /// <param name="entries">The entries to save.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        public override Task<int> SaveChangesAsync(IList<IUpdateEntry> entries, CancellationToken cancellationToken = default)
            => this.saveChangesAsync(entries, cancellationToken);

    }
}
