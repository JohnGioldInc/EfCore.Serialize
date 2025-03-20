// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazorApp1.Controllers
{
    using System.Collections.Generic;
    using System.Text.Json;
    using BlazorApp1.Data;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore.Update;
    using Remote.Linq.EntityFrameworkCore;
    using Remote.Linq.Text.Json;

    /// <summary>
    /// DataController.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly BlazorApp1Context blazorApp1Context;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataController"/> class.
        /// </summary>
        /// <param name="blazorApp1Context">BlazorApp1Context.</param>
        public DataController(BlazorApp1Context blazorApp1Context)
        {
            this.blazorApp1Context = blazorApp1Context;
            this.jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            }.ConfigureRemoteLinq();
        }

        /// <summary>
        /// Get Data.
        /// </summary>
        /// <param name="serializedExpression">Raw Json string (made possible by RawJsonBodyInputFormatter).</param>
        /// <returns>Data.</returns>
        [HttpPost("query")]
        public ActionResult<IEnumerable<object>> Query([FromBody] string serializedExpression)
            => this.Ok(
               JsonSerializer.Deserialize<Remote.Linq.Expressions.Expression>(serializedExpression, this.jsonSerializerOptions)
                !.ExecuteWithEntityFrameworkCore(this.blazorApp1Context));

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
