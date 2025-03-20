// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Update;

/// <summary>
/// Extension methods for SerializeDbContext
/// </summary>
public static class SerializeDbContextExtension
{

    /// <summary>
    /// Save changes async From Serialize Changes
    /// </summary>
    /// <param name="dbContext">The database context to save changes for.</param>
    /// <param name="entries">The entries to be updated.</param>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether all changes should be accepted on success.</param>
    /// <param name="configureAwait">Indicates whether to configure await behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public static async Task<int> SaveChangesAsync(this DbContext dbContext, IEnumerable<IUpdateEntry> entries,
        CancellationToken cancellationToken = default,
        bool acceptAllChangesOnSuccess = true,
        bool configureAwait = false)
    {
#pragma warning disable EF1001 // Internal EF Core API usage.
        foreach (var entry in entries.Cast<InternalEntityEntry>())
        {
            var entityEntry = dbContext.Attach(entry.Entity);
            dbContext.Entry(entityEntry).State = entry.EntityState;
        }
#pragma warning restore EF1001 // Internal EF Core API usage.

        return await dbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(configureAwait);
    }

    /// <summary>
    /// Return Query Results From Serialized Expression
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext to query upon</typeparam>
    /// <param name="dbContext">The database context to query upon.</param>
    /// <param name="deserializer">Deserialization Function</param>
    /// <param name="serializedExpression">The serialized expression to be deserialized and executed.</param>
    /// <returns>query results of serialized expression</returns>
    public static IEnumerable<object> FromSerializedExpression<TDbContext>(this TDbContext dbContext, Func<string,Expression> deserializer, string serializedExpression)
        where TDbContext : DbContext
    {
        var expression = deserializer(serializedExpression);

        expression = new ReplaceWithEntityQueryRootExpressionVisitor(dbContext).Visit(expression);

        var genericArgumentType = expression.Type.GetGenericArguments().FirstOrDefault();

        var queryableType = typeof(IQueryable<>).MakeGenericType(genericArgumentType!);
        var funcType = typeof(Func<>).MakeGenericType(queryableType);
        var lambda = Expression.Lambda(funcType, expression);

        var fromExpressionMethod = typeof(TDbContext)
            .GetMethod(nameof(DbContext.FromExpression))
            ?.MakeGenericMethod(genericArgumentType!);

        var query = fromExpressionMethod!.Invoke(dbContext, new object[] { lambda });

        var result = (query as IEnumerable<object>)?.ToList();

        return result!;
    }



    private class ReplaceWithEntityQueryRootExpressionVisitor : ExpressionVisitor
    {
        private readonly DbContext dbContext;

        public ReplaceWithEntityQueryRootExpressionVisitor(DbContext dbContext)
            => this.dbContext = dbContext;

        public Expression Rewrite(Expression expression)
            => Visit(expression);

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
            => parameterExpression.Name == "queryable"
            && parameterExpression.Type.IsGenericType 
            && parameterExpression.Type.GetGenericTypeDefinition() == typeof(IQueryable<>)
                ? new EntityQueryRootExpression(dbContext.Model.GetEntityTypes().First(et => et.ClrType == parameterExpression.Type.GetGenericArguments().First()))
                : base.VisitParameter(parameterExpression);

    }
}

