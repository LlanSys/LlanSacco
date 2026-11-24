using FluentValidation;
using LS.SharedKernel.Extensions;

namespace LS.SharedKernel.Validation.Extensions;

public static class EnumRuleExtensions
{
    /// <summary>
    /// Validates a string that a handler converts with <c>ToEnum&lt;TEnum&gt;()</c>. FluentValidation's
    /// <c>IsInEnum()</c> only works on enum-typed properties and rejects every string value.
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsEnumValue<T, TEnum>(this IRuleBuilder<T, string> ruleBuilder)
        where TEnum : struct, Enum
        => ruleBuilder.Must(value => value.TryToEnum<TEnum>(out _));
}
