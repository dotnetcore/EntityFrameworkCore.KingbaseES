using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using Kdbndp.EntityFrameworkCore.KingbaseES.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Query.Expressions.Internal;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;
using static Kdbndp.EntityFrameworkCore.KingbaseES.Utilities.Statics;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

public class KdbndpByteArrayMethodTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public KdbndpByteArrayMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        Check.NotNull(method, nameof(method));
        Check.NotNull(arguments, nameof(arguments));

        if (method.IsGenericMethod && arguments[0].TypeMapping is KdbndpByteArrayTypeMapping typeMapping)
        {
            // Note: we only translate if the array argument is a column mapped to bytea. There are various other
            // cases (e.g. Where(b => new byte[] { 1, 2, 3 }.Contains(b.SomeByte))) where we prefer to translate via
            // regular KingbaseES array logic.
            if (method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains))
            {
                var source = arguments[0];

                // We have a byte value, but we need a bytea for KingbaseES POSITION.
                var value = arguments[1] is SqlConstantExpression constantValue
                    ? (SqlExpression)_sqlExpressionFactory.Constant(new[] { (byte)constantValue.Value! }, typeMapping)
                    // Create bytea from non-constant byte: SELECT set_byte('\x00', 0, 8::smallint);
                    : _sqlExpressionFactory.Function(
                        "set_byte",
                        new[]
                        {
                            _sqlExpressionFactory.Constant(new[] { (byte)0 }, typeMapping),
                            _sqlExpressionFactory.Constant(0),
                            arguments[1]
                        },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[3],
                        typeof(byte[]),
                        typeMapping);

                return _sqlExpressionFactory.GreaterThan(
                    PostgresFunctionExpression.CreateWithArgumentSeparators(
                        "position",
                        new[] { value, source },
                        new[] { "IN" },   // POSITION(x IN y)
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        builtIn: true,
                        typeof(int),
                        null),
                    _sqlExpressionFactory.Constant(0));
            }

            if (method.GetGenericMethodDefinition().Equals(EnumerableMethods.FirstWithoutPredicate))
            {
                return _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function(
                        "get_byte",
                        new[] { arguments[0], _sqlExpressionFactory.Constant(0) },
                        nullable: true,
                        argumentsPropagateNullability: TrueArrays[2],
                        typeof(byte)),
                    method.ReturnType);
            }
        }

        return null;
    }
}
