// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazorApp1.Controllers
{
    using System.Linq.Expressions;
    using System.Text.Json.Serialization;
    using BlazorApp1.Data;
    using JohnGoldInc.EntityFrameworkCore.Serialize;
    using JohnGoldInc.EntityFrameworkCore.Serialize.Serializers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore.Update;
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
            var expression = this.serializer.DeserializeText(serializedExpression) as LambdaExpression;
            if (expression == null)
            {
                return this.BadRequest("Invalid expression");
            }

            var genericArgumentType = expression.ReturnType.GetGenericArguments().FirstOrDefault();
            if (genericArgumentType == null)
            {
                return this.BadRequest("Invalid expression return type");
            }

            var fromExpressionMethod = typeof(BlazorApp1Context)
                .GetMethod(nameof(BlazorApp1Context.FromExpression))
                ?.MakeGenericMethod(genericArgumentType);

            if (fromExpressionMethod == null)
            {
                return this.StatusCode(500, "Unable to find FromExpression method");
            }

            var query = fromExpressionMethod.Invoke(this.blazorApp1Context, new object[] { expression });
            var result = ((IQueryable<object>)query!).ToList();

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
