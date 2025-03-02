// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazorApp1.Controllers
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using BlazorApp1.Data;
    using JohnGoldInc.EntityFrameworkCore.Serialize;
    using JohnGoldInc.EntityFrameworkCore.Serialize.Serializers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore.Update;
    using Serialize.Linq.Interfaces;
    using Serialize.Linq.Serializers;

    /// <summary>
    /// DataController.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly BlazorApp1Context blazorApp1Context;
        private readonly EfCoreExpressionSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataController"/> class.
        /// </summary>
        /// <param name="blazorApp1Context">BlazorApp1Context.</param>
        public DataController(BlazorApp1Context blazorApp1Context)
        {
            this.blazorApp1Context = blazorApp1Context;
            this.serializer = new EfCoreExpressionSerializer(new JsonSerializer());
        }

        /// <summary>
        /// Get Data.
        /// </summary>
        /// <param name="serializedExpression">Raw Json string (made possible by RawJsonBodyInputFormatter).</param>
        /// <returns>Data.</returns>
        [HttpPost("query")]
        public ActionResult<IEnumerable<object>> Query([FromBody] string serializedExpression)
        {
            IExpressionContext context = new SerializeExpressionContext(this.blazorApp1Context);

            var expression = this.serializer.DeserializeText(serializedExpression, context);
            if (expression == null)
            {
                return this.BadRequest("Invalid expression");
            }

            var genericArgumentType = expression.Type.GetGenericArguments().FirstOrDefault();
            if (genericArgumentType == null)
            {
                return this.BadRequest("Invalid expression return type");
            }

            var queryableType = typeof(IQueryable<>).MakeGenericType(genericArgumentType);
            var funcType = typeof(Func<>).MakeGenericType(queryableType);
            var lambda = Expression.Lambda(funcType, expression);

            var fromExpressionMethod = typeof(BlazorApp1Context)
                .GetMethod(nameof(BlazorApp1Context.FromExpression))
                ?.MakeGenericMethod(genericArgumentType);

            if (fromExpressionMethod == null)
            {
                return this.StatusCode(500, "Unable to find FromExpression method");
            }

            var query = fromExpressionMethod.Invoke(this.blazorApp1Context, new object[] { lambda });
            if (query == null)
            {
                return this.StatusCode(500, "Query result is null");
            }

            var result = (query as IEnumerable<object>)?.ToList();

            return this.Ok(result);
        }

        /// <summary>
        /// Save Data.
        /// </summary>
        /// <param name="entities">IEnumerable &lt; IUpdateEntry &gt;.</param>
        /// <returns>saved rows.</returns>
        [HttpPost("save")]
        public async ValueTask<ActionResult<int?>> SaveChangesAsync([FromBody] IEnumerable<IUpdateEntry> entities)
            => this.Ok(await this.blazorApp1Context.SaveChangesAsync(entities));
    }
}
