// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Serialize.Linq;
using Serialize.Linq.Interfaces;

namespace JohnGoldInc.EntityFrameworkCore.Serialize
{
    /// <summary>
    /// Provides a context for serializing expressions with Entity Framework Core.
    /// </summary>
    public class SerializeExpressionContext : ExpressionContext, IExpressionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializeExpressionContext"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public SerializeExpressionContext(DbContext dbContext)
            => this.DbContext = dbContext;

        /// <summary>
        /// Gets the database context.
        /// </summary>
        public DbContext DbContext { get; }
    }
}
