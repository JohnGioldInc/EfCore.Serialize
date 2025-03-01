// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
namespace BlazorApp1.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// BlazorApp1Context.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BlazorApp1Context"/> class.
    /// </remarks>
    /// <param name="options">options.</param>
    public class BlazorApp1Context(DbContextOptions<BlazorApp1Context> options)
        : DbContext(options)
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        /// <summary>
        /// Gets or sets WeatherForecast.
        /// </summary>
        public DbSet<WeatherForecast> WeatherForecast { get; set; }

        /// <summary>
        /// Gets or sets PrecipitationByHour.
        /// </summary>
        public DbSet<PrecipitationByHour> PrecipitationByHour { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    }

#pragma warning disable SA1402 // File may only contain a single type
    /// <summary>
    ///     WeatherForecast.
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>
        /// Gets or sets Date.
        /// </summary>
        [Key]
        public DateOnly Date { get; set; }

        /// <summary>
        /// Gets or sets TemperatureC.
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// Gets or sets Summary.
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Gets TemperatureF.
        /// </summary>
        public int TemperatureF => 32 + (int)(this.TemperatureC / 0.5556);

        /// <summary>
        /// Gets or sets PrecipitationByHour.
        /// </summary>
        [ForeignKey("Date")]
        public ICollection<PrecipitationByHour>? PrecipitationByHour { get; set; }
    }

    /// <summary>
    ///    PrecipitationByHour.
    /// </summary>
    [PrimaryKey(nameof(Date), nameof(Hour))]
    public class PrecipitationByHour
    {
        /// <summary>
        /// Gets or sets Date.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Gets or sets Hour.
        /// </summary>
        public int Hour { get; set; }

        /// <summary>
        /// Gets or sets Chance.
        /// </summary>
        public int Chance { get; set; }
    }
#pragma warning restore SA1402 // File may only contain a single type
    }
