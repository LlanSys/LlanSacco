using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using LS.SharedKernel.Features.Shared.Lookups.Dtos;

namespace LS.SharedKernel.Extensions;

public static class EnumExtensions
{
    public static string ToDisplayString(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.GetType()
                .GetField(value.ToString())
                ?.GetCustomAttribute<DescriptionAttribute>()
                ?.Description ?? value.ToString();
    }

    public static T ToEnum<T>(this string description) where T : struct, Enum
        => description.TryToEnum<T>(out var value)
            ? value
            : throw new ArgumentException($"No enum member with description '{description}' found in {typeof(T).Name}");

    /// <summary>
    /// Matches a member by its <see cref="DescriptionAttribute"/> or, failing that, its exact name.
    /// Validators use this so they accept exactly what <see cref="ToEnum{T}"/> can convert.
    /// </summary>
    public static bool TryToEnum<T>(this string? description, out T value) where T : struct, Enum
    {
        if (description is not null)
        {
            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if ((attribute != null && attribute.Description == description) || field.Name == description)
                {
                    value = (T)field.GetValue(null)!;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    public static Collection<LookupResponse> ToLookupResponses<T>() where T : struct, Enum
    {
        return [.. Enum.GetValues<T>().Select(static e => new LookupResponse(
            Convert.ToInt32(e, CultureInfo.InvariantCulture),
            e.ToString(),
            e.ToDisplayString()

        ))];
    }

    public static IReadOnlyList<(T Value, string Display)> GetSelectList<T>()
        where T : struct, Enum
        => [.. Enum.GetValues<T>().Select(e => (e, e.ToDisplayString()))];
}

