using System.Net;
using System.Net.NetworkInformation;
using Kdbndp.EntityFrameworkCore.KingbaseES.Query.Expressions;
using static Kdbndp.EntityFrameworkCore.KingbaseES.Utilities.Statics;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

/// <summary>
///     Provides translation services for operators and functions of KingbaseES network typess (cidr, inet, macaddr, macaddr8).
/// </summary>
/// <remarks>
///     See: https://www.KingbaseES.org/docs/current/static/functions-net.html
/// </remarks>
public class KdbndpNetworkTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo IPAddressParse =
        typeof(IPAddress).GetRuntimeMethod(nameof(IPAddress.Parse), [typeof(string)])!;

    private static readonly MethodInfo PhysicalAddressParse =
        typeof(PhysicalAddress).GetRuntimeMethod(nameof(PhysicalAddress.Parse), [typeof(string)])!;

    private readonly KdbndpSqlExpressionFactory _sqlExpressionFactory;

    private readonly RelationalTypeMapping _inetMapping;
    private readonly RelationalTypeMapping _cidrMapping;
    private readonly RelationalTypeMapping _macaddr8Mapping;
    private readonly RelationalTypeMapping _longAddressMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public KdbndpNetworkTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        KdbndpSqlExpressionFactory sqlExpressionFactory,
        IModel model)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _inetMapping = typeMappingSource.FindMapping("inet")!;
        _cidrMapping = typeMappingSource.FindMapping("cidr")!;
        _macaddr8Mapping = typeMappingSource.FindMapping("macaddr8")!;
        _longAddressMapping = typeMappingSource.FindMapping(typeof(long), model)!;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method == IPAddressParse)
        {
            return _sqlExpressionFactory.Convert(arguments[0], typeof(IPAddress));
        }

        if (method == PhysicalAddressParse)
        {
            return _sqlExpressionFactory.Convert(arguments[0], typeof(PhysicalAddress));
        }

        if (method.DeclaringType == typeof(KdbndpNetworkDbFunctionsExtensions))
        {
            var paramType = method.GetParameters()[1].ParameterType;

            if (paramType == typeof(KdbndpInet))
            {
                return TranslateInetExtensionMethod(method, arguments);
            }

            if (paramType == typeof(KdbndpCidr))
            {
                return TranslateCidrExtensionMethod(method, arguments);
            }

            if (paramType == typeof(PhysicalAddress))
            {
                return TranslateMacaddrExtensionMethod(method, arguments);
            }
        }

        return null;
    }

    private SqlExpression? TranslateInetExtensionMethod(MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        => method.Name switch
        {
            nameof(KdbndpNetworkDbFunctionsExtensions.LessThan)
                => new SqlBinaryExpression(
                    ExpressionType.LessThan,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(KdbndpInet),
                    _inetMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.LessThanOrEqual)
                => new SqlBinaryExpression(
                    ExpressionType.LessThanOrEqual,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(KdbndpInet),
                    _inetMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.GreaterThanOrEqual)
                => new SqlBinaryExpression(
                    ExpressionType.GreaterThanOrEqual,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(KdbndpInet),
                    _inetMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.GreaterThan)
                => new SqlBinaryExpression(
                    ExpressionType.GreaterThan,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(KdbndpInet),
                    _inetMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.ContainedBy)
                => _sqlExpressionFactory.ContainedBy(arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.ContainedByOrEqual)
                => _sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.NetworkContainedByOrEqual, arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.Contains)
                => _sqlExpressionFactory.Contains(arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.ContainsOrEqual)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.NetworkContainsOrEqual, arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.ContainsOrContainedBy)
                => _sqlExpressionFactory.MakePostgresBinary(
                    PgExpressionType.NetworkContainsOrContainedBy, arguments[1], arguments[2]),

            nameof(KdbndpNetworkDbFunctionsExtensions.BitwiseNot)
                => new SqlUnaryExpression(
                    ExpressionType.Not,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    typeof(KdbndpInet),
                    _inetMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.BitwiseAnd)
                => new SqlBinaryExpression(
                    ExpressionType.And,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(KdbndpInet),
                    _inetMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.BitwiseOr)
                => new SqlBinaryExpression(
                    ExpressionType.Or,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(KdbndpInet),
                    _inetMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.Add)
                => new SqlBinaryExpression(
                    ExpressionType.Add,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(KdbndpInet),
                    _inetMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.Subtract) when arguments[2].Type == typeof(long)
                => new SqlBinaryExpression(
                    ExpressionType.Subtract,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(KdbndpInet),
                    _inetMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.Subtract)
                => new SqlBinaryExpression(
                    ExpressionType.Subtract,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    typeof(long),
                    _longAddressMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.Abbreviate)
                => NullPropagatingFunction("abbrev", [arguments[1]], typeof(string)),
            nameof(KdbndpNetworkDbFunctionsExtensions.Broadcast)
                => NullPropagatingFunction("broadcast", [arguments[1]], typeof(IPAddress), _inetMapping),
            nameof(KdbndpNetworkDbFunctionsExtensions.Family)
                => NullPropagatingFunction("family", [arguments[1]], typeof(int)),
            nameof(KdbndpNetworkDbFunctionsExtensions.Host)
                => NullPropagatingFunction("host", [arguments[1]], typeof(string)),
            nameof(KdbndpNetworkDbFunctionsExtensions.HostMask)
                => NullPropagatingFunction("hostmask", [arguments[1]], typeof(IPAddress), _inetMapping),
            nameof(KdbndpNetworkDbFunctionsExtensions.MaskLength)
                => NullPropagatingFunction("masklen", [arguments[1]], typeof(int)),
            nameof(KdbndpNetworkDbFunctionsExtensions.Netmask)
                => NullPropagatingFunction("netmask", [arguments[1]], typeof(IPAddress), _inetMapping),
            nameof(KdbndpNetworkDbFunctionsExtensions.Network)
                => NullPropagatingFunction("network", [arguments[1]], typeof((IPAddress Address, int Subnet)), _cidrMapping),
            nameof(KdbndpNetworkDbFunctionsExtensions.SetMaskLength)
                => NullPropagatingFunction(
                    "set_masklen", [arguments[1], arguments[2]], arguments[1].Type, arguments[1].TypeMapping),
            nameof(KdbndpNetworkDbFunctionsExtensions.Text)
                => NullPropagatingFunction("text", [arguments[1]], typeof(string)),
            nameof(KdbndpNetworkDbFunctionsExtensions.SameFamily)
                => NullPropagatingFunction("inet_same_family", [arguments[1], arguments[2]], typeof(bool)),
            nameof(KdbndpNetworkDbFunctionsExtensions.Merge)
                => NullPropagatingFunction(
                    "inet_merge", [arguments[1], arguments[2]], typeof((IPAddress Address, int Subnet)), _cidrMapping),

            _ => null
        };

    private SqlExpression? TranslateCidrExtensionMethod(MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        => method.Name switch
        {
            nameof(KdbndpNetworkDbFunctionsExtensions.Abbreviate)
                => NullPropagatingFunction("abbrev", [arguments[1]], typeof(string)),
            nameof(KdbndpNetworkDbFunctionsExtensions.SetMaskLength)
                => NullPropagatingFunction(
                    "set_masklen", [arguments[1], arguments[2]], arguments[1].Type, arguments[1].TypeMapping),

            _ => null
        };

    private SqlExpression? TranslateMacaddrExtensionMethod(MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        => method.Name switch
        {
            nameof(KdbndpNetworkDbFunctionsExtensions.LessThan)
                => _sqlExpressionFactory.LessThan(arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.LessThanOrEqual)
                => _sqlExpressionFactory.LessThanOrEqual(arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.GreaterThanOrEqual)
                => _sqlExpressionFactory.GreaterThanOrEqual(arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.GreaterThan)
                => _sqlExpressionFactory.GreaterThan(arguments[1], arguments[2]),

            nameof(KdbndpNetworkDbFunctionsExtensions.BitwiseNot)
                => _sqlExpressionFactory.Not(arguments[1]),
            nameof(KdbndpNetworkDbFunctionsExtensions.BitwiseAnd)
                => _sqlExpressionFactory.And(arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.BitwiseOr)
                => _sqlExpressionFactory.Or(arguments[1], arguments[2]),

            nameof(KdbndpNetworkDbFunctionsExtensions.Truncate) => NullPropagatingFunction(
                "trunc", [arguments[1]], typeof(PhysicalAddress), arguments[1].TypeMapping),
            nameof(KdbndpNetworkDbFunctionsExtensions.Set7BitMac8) => NullPropagatingFunction(
                "macaddr8_set7bit", [arguments[1]], typeof(PhysicalAddress), _macaddr8Mapping),

            _ => null
        };

    private SqlExpression NullPropagatingFunction(
        string name,
        SqlExpression[] arguments,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
        => _sqlExpressionFactory.Function(
            name,
            arguments,
            nullable: true,
            argumentsPropagateNullability: TrueArrays[arguments.Length],
            returnType,
            typeMapping);
}
