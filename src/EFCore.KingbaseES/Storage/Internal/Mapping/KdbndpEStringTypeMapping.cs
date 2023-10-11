using Microsoft.EntityFrameworkCore.Storage;
using Kdbndp.EntityFrameworkCore.KingbaseES.Utilities;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

/// <summary>
/// Represents a so-called KingbaseES E-string literal string, which allows C-style escape sequences.
/// This is a "virtual" type mapping which is never returned by <see cref="KdbndpTypeMappingSource"/>.
/// It is only used internally by some method translators to produce literal strings.
/// </summary>
/// <remarks>
/// See https://www.KingbaseES.org/docs/current/sql-syntax-lexical.html#SQL-SYNTAX-CONSTANTS
/// </remarks>
public class KdbndpEStringTypeMapping : StringTypeMapping
{
    public KdbndpEStringTypeMapping() : base("does_not_exist", System.Data.DbType.String) {}

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"E'{EscapeSqlLiteral((string)value)}'";
}
