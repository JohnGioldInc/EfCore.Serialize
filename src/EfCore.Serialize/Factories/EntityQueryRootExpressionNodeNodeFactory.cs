// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using JohnGoldInc.EntityFrameworkCore.Serialize.Nodes;
using Microsoft.EntityFrameworkCore.Query;
using Serialize.Linq.Factories;
using Serialize.Linq.Nodes;

namespace JohnGoldInc.EntityFrameworkCore.Serialize.Factories
{
    /// <summary>
    /// Factory class for creating EntityQueryRootExpressionNode nodes.
    /// </summary>
    public class EntityQueryRootExpressionNodeNodeFactory : NodeFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityQueryRootExpressionNodeNodeFactory"/> class.
        /// </summary>
        /// <param name="factorySettings">The settings to be used by the factory.</param>
        public EntityQueryRootExpressionNodeNodeFactory(FactorySettings? factorySettings) : base(factorySettings)
        {
        }

        /// <inheritdoc/>
        public override ExpressionNode Create(Expression expression)
        {
            if (expression is EntityQueryRootExpression entityQueryRootExpression) return new EntityQueryRootExpressionNode(this, entityQueryRootExpression);
            return base.Create(expression);
        }
    }
}
