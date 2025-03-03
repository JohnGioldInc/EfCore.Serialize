// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using JohnGoldInc.EntityFrameworkCore.Serialize.Factories;
using JohnGoldInc.EntityFrameworkCore.Serialize.Nodes;
using Microsoft.EntityFrameworkCore.Query;
using Serialize.Linq.Factories;
using Serialize.Linq.Interfaces;
using Serialize.Linq.Nodes;
using Serialize.Linq.Serializers;

namespace JohnGoldInc.EntityFrameworkCore.Serialize.Serializers
{
    /// <summary>
    /// Serializer for EF Core expressions.
    /// </summary>
    public class EfCoreExpressionSerializer : ExpressionSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfCoreExpressionSerializer"/> class.
        /// </summary>
        /// <param name="serializer">The serializer to use.</param>
        /// <param name="factorySettings">The factory settings to use.</param>
        public EfCoreExpressionSerializer(ISerializer serializer, FactorySettings? factorySettings = null)
            : base(serializer, factorySettings)
        {
            serializer.AddKnownType(typeof(EntityQueryRootExpressionNode));
            base.AddKnownType(typeof(EntityQueryRootExpressionNode));
        }

        /// <inheritdoc/>
        protected override INodeFactory CreateFactory(Expression expression, FactorySettings? factorySettings)
            => new EntityQueryRootExpressionNodeNodeFactory(factorySettings);
      
    }
}
