using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Kdbndp.EntityFrameworkCore.KingbaseES.Infrastructure.Internal;
using static Kdbndp.EntityFrameworkCore.KingbaseES.Utilities.Statics;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

/// <summary>
/// Provides translation services for KingbaseES UUID functions.
/// </summary>
/// <remarks>
/// See: https://www.KingbaseES.org/docs/current/datatype-uuid.html
/// </remarks>
public class KdbndpNewGuidTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo MethodInfo = typeof(Guid).GetRuntimeMethod(nameof(Guid.NewGuid), Array.Empty<Type>())!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly string _uuidGenerationFunction;

    public KdbndpNewGuidTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
        IKdbndpSingletonOptions KdbndpSingletonOptions)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _uuidGenerationFunction = KdbndpSingletonOptions.PostgresVersion.AtLeast(13) ? "gen_random_uuid" : "uuid_generate_v4";
    }

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => MethodInfo.Equals(method)
            ? _sqlExpressionFactory.Function(
                _uuidGenerationFunction,
                Array.Empty<SqlExpression>(),
                nullable: false,
                argumentsPropagateNullability: FalseArrays[0],
                method.ReturnType)
            : null;
}
