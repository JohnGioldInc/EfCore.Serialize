// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc.Formatters;

/// <summary>
/// RawJsonBodyInputFormatter.
/// </summary>
public class RawJsonBodyInputFormatter : InputFormatter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RawJsonBodyInputFormatter"/> class.
    /// </summary>
    public RawJsonBodyInputFormatter()
    {
        this.SupportedMediaTypes.Add("application/json");
    }

    /// <inheritdoc/>
    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    {
        var request = context.HttpContext.Request;
        using (var reader = new StreamReader(request.Body))
        {
            var content = await reader.ReadToEndAsync();
            return await InputFormatterResult.SuccessAsync(content);
        }
    }

    /// <inheritdoc/>
    protected override bool CanReadType(Type type)
    {
        return type == typeof(string);
    }
}
