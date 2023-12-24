using System.Diagnostics.CodeAnalysis;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

// ReSharper disable once CheckNamespace
namespace System.Reflection;

internal static class TypeExtensions
{
    internal static bool IsGenericList(this Type? type)
        => type is not null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

    internal static bool IsArrayOrGenericList(this Type type)
        => type.IsArray || type.IsGenericList();

    internal static bool TryGetElementType(this Type type, [NotNullWhen(true)] out Type? elementType)
    {
        elementType = type.IsArray
            ? type.GetElementType()
            : type.IsGenericList()
                ? type.GetGenericArguments()[0]
                : null;
        return elementType is not null;
    }

    internal static bool IsRange(this Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KdbndpRange<>)
            || type is { Name: "Interval" or "DateInterval", Namespace: "NodaTime" };

    internal static bool IsMultirange(this Type type)
        => type.TryGetElementType(out var elementType) && elementType.IsRange();


    internal static bool TryGetRangeSubtype(this Type type, [NotNullWhen(true)] out Type? subtypeType)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KdbndpRange<>))
        {
            subtypeType = type.GetGenericArguments()[0];
            return true;
        }

        subtypeType = null;
        return false;
    }

    internal static bool TryGetMultirangeSubtype(this Type type, [NotNullWhen(true)] out Type? subtypeType)
    {
        if (type.TryGetElementType(out var elementType) && elementType.TryGetRangeSubtype(out subtypeType))
        {
            return true;
        }

        subtypeType = null;
        return false;
    }

    public static PropertyInfo? FindIndexerProperty(this Type type)
    {
        var defaultPropertyAttribute = type.GetCustomAttributes<DefaultMemberAttribute>().FirstOrDefault();

        return defaultPropertyAttribute is null
            ? null
            : type.GetRuntimeProperties()
                .FirstOrDefault(pi => pi.Name == defaultPropertyAttribute.MemberName && pi.GetIndexParameters().Length == 1);
    }
}
