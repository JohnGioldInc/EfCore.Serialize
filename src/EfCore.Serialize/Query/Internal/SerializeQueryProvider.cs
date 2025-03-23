// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore.Query;

namespace JohnGoldInc.EntityFrameworkCore.Serialize.Query.Internal
{
    /// <inheritdoc />
    public class SerializeQueryProvider : IAsyncQueryProvider
    {
        private readonly Func<string, CancellationToken, Task<string>> dataProvider;
        private readonly Func<Expression, string> serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializeQueryProvider"/> class.
        /// </summary>
        public SerializeQueryProvider(Func<string, CancellationToken, Task<string>> dataProvider, Func<Expression, string> serializer)
        {
            this.dataProvider = dataProvider;
            this.serializer = serializer;
        }

        private static readonly MethodInfo ExecuteAsyncMethod = typeof(SerializeQueryProvider).GetMethod("ExecuteAsync", new[] { typeof(Expression), typeof(CancellationToken) })!;

        /// <inheritdoc />
        public IQueryable CreateQuery(Expression expression)
            => (IQueryable)ExecuteAsyncMethod
            .MakeGenericMethod(typeof(IQueryable<>)
                .MakeGenericType( expression.Type.GenericTypeArguments.First()))
            .Invoke(this, new object[] { expression, CancellationToken.None })!;

        /// <inheritdoc />
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
           => (IQueryable<TElement>)ExecuteAsyncMethod
            .MakeGenericMethod(typeof(IQueryable<TElement>))
            .Invoke(this, new object[] { expression, CancellationToken.None })!;

        /// <inheritdoc />
        public object? Execute(Expression expression)
            => CreateQuery(expression);

        /// <inheritdoc />
        public TResult Execute<TResult>(Expression expression)
            => ExecuteAsync<TResult>(expression);

        /// <inheritdoc />
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var unrootExpression = ReplaceEntityQueryRootExpressionVisitor.Instance.Visit(expression);
            var json = serializer(unrootExpression);
            var data = dataProvider(json, cancellationToken);

            var result = typeof(QueryingEnumerable<>)
                .MakeGenericType(typeof(TResult).GetGenericArguments().First())
                .GetConstructor(new[] { typeof(Task<string>)})
                ?.Invoke(new object[] { data})!;

            return (TResult)result;
        }

        private class ReplaceEntityQueryRootExpressionVisitor : ExpressionVisitor
        {
            public static readonly ReplaceEntityQueryRootExpressionVisitor Instance = _Instance ??= new ReplaceEntityQueryRootExpressionVisitor();
            private static ReplaceEntityQueryRootExpressionVisitor _Instance;

            public Expression Rewrite(Expression expression)
                => Visit(expression);

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
    }
}
