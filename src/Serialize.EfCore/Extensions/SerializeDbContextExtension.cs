// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Update;

namespace JohnGoldInc.EntityFrameworkCore.Serialize
{
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
    }
}
