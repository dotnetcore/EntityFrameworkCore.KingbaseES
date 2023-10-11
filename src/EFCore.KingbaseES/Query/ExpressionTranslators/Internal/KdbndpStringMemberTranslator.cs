using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using static Kdbndp.EntityFrameworkCore.KingbaseES.Utilities.Statics;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

/// <summary>
/// Translates <see cref="M:string.Length"/> to 'length(text)'.
/// </summary>
public class KdbndpStringMemberTranslator : IMemberTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public KdbndpStringMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => member.Name == nameof(string.Length) && instance?.Type == typeof(string)
            ? _sqlExpressionFactory.Convert(
                _sqlExpressionFactory.Function(
                    "length",
                    new[] { instance },
                    nullable: true,
                    argumentsPropagateNullability: TrueArrays[1],
                    typeof(long)),
                returnType)
            : null;
}
