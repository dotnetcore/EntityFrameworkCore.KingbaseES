using System.Net;
using System.Net.NetworkInformation;
using Kdbndp.EntityFrameworkCore.KingbaseES.Query.Expressions;
using static Kdbndp.EntityFrameworkCore.KingbaseES.Utilities.Statics;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Query.ExpressionTranslators.Internal;

/// <summary>
/// Provides translation services for operators and functions of KingbaseES network typess (cidr, inet, macaddr, macaddr8).
/// </summary>
/// <remarks>
/// See: https://www.KingbaseES.org/docs/current/static/functions-net.html
/// </remarks>
public class KdbndpNetworkTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo IPAddressParse =
        typeof(IPAddress).GetRuntimeMethod(nameof(IPAddress.Parse), new[] { typeof(string) })!;

    private static readonly MethodInfo PhysicalAddressParse =
        typeof(PhysicalAddress).GetRuntimeMethod(nameof(PhysicalAddress.Parse), new[] { typeof(string) })!;

    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly KdbndpSqlExpressionFactory _sqlExpressionFactory;

    private readonly RelationalTypeMapping _inetMapping;
    private readonly RelationalTypeMapping _cidrMapping;
    private readonly RelationalTypeMapping _macaddr8Mapping;
    private readonly RelationalTypeMapping _longAddressMapping;

    public KdbndpNetworkTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        KdbndpSqlExpressionFactory sqlExpressionFactory,
        IModel model)
    {
        _typeMappingSource = typeMappingSource;
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

        if (method.DeclaringType != typeof(KdbndpNetworkDbFunctionsExtensions))
        {
            return null;
        }

        return method.Name switch
        {
            nameof(KdbndpNetworkDbFunctionsExtensions.LessThan)
                => _sqlExpressionFactory.LessThan(arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.LessThanOrEqual)
                => _sqlExpressionFactory.LessThanOrEqual(arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.GreaterThanOrEqual)
                => _sqlExpressionFactory.GreaterThanOrEqual(arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.GreaterThan)
                => _sqlExpressionFactory.GreaterThan(arguments[1], arguments[2]),

            nameof(KdbndpNetworkDbFunctionsExtensions.ContainedBy)
                => _sqlExpressionFactory.ContainedBy(arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.ContainedByOrEqual)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.NetworkContainedByOrEqual, arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.Contains)
                => _sqlExpressionFactory.Contains(arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.ContainsOrEqual)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.NetworkContainsOrEqual, arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.ContainsOrContainedBy)
                => _sqlExpressionFactory.MakePostgresBinary(PgExpressionType.NetworkContainsOrContainedBy, arguments[1], arguments[2]),

            nameof(KdbndpNetworkDbFunctionsExtensions.BitwiseNot)            => new SqlUnaryExpression(ExpressionType.Not,
                arguments[1],
                arguments[1].Type,
                arguments[1].TypeMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.BitwiseAnd) => _sqlExpressionFactory.And(arguments[1], arguments[2]),
            nameof(KdbndpNetworkDbFunctionsExtensions.BitwiseOr)  => _sqlExpressionFactory.Or(arguments[1], arguments[2]),

            // Add/Subtract accept inet + int, so we can't use the default type mapping inference logic which assumes
            // same-typed operands
            nameof(KdbndpNetworkDbFunctionsExtensions.Add)
                => new SqlBinaryExpression(
                    ExpressionType.Add,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    arguments[1].Type,
                    arguments[1].TypeMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.Subtract) when arguments[2].Type == typeof(int)
                => new SqlBinaryExpression(
                    ExpressionType.Subtract,
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]),
                    _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]),
                    arguments[1].Type,
                    arguments[1].TypeMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.Subtract)
                when arguments[2].Type == typeof(IPAddress) || arguments[2].Type == typeof((IPAddress, int))
                => new SqlBinaryExpression(
                    ExpressionType.Subtract,
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[1], ExpressionExtensions.InferTypeMapping(arguments[1], arguments[2])),
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[2], ExpressionExtensions.InferTypeMapping(arguments[1], arguments[2])),
                    arguments[1].Type,
                    _longAddressMapping),

            nameof(KdbndpNetworkDbFunctionsExtensions.Abbreviate)    => NullPropagatingFunction("abbrev",           new[] { arguments[1] }, typeof(string)),
            nameof(KdbndpNetworkDbFunctionsExtensions.Broadcast)     => NullPropagatingFunction("broadcast",        new[] { arguments[1] }, typeof(IPAddress), _inetMapping),
            nameof(KdbndpNetworkDbFunctionsExtensions.Family)        => NullPropagatingFunction("family",           new[] { arguments[1] }, typeof(int)),
            nameof(KdbndpNetworkDbFunctionsExtensions.Host)          => NullPropagatingFunction("host",             new[] { arguments[1] }, typeof(string)),
            nameof(KdbndpNetworkDbFunctionsExtensions.HostMask)      => NullPropagatingFunction("hostmask",         new[] { arguments[1] }, typeof(IPAddress), _inetMapping),
            nameof(KdbndpNetworkDbFunctionsExtensions.MaskLength)    => NullPropagatingFunction("masklen",          new[] { arguments[1] }, typeof(int)),
            nameof(KdbndpNetworkDbFunctionsExtensions.Netmask)       => NullPropagatingFunction("netmask",          new[] { arguments[1] }, typeof(IPAddress), _inetMapping),
            nameof(KdbndpNetworkDbFunctionsExtensions.Network)       => NullPropagatingFunction("network",          new[] { arguments[1] }, typeof((IPAddress Address, int Subnet)), _cidrMapping),
            nameof(KdbndpNetworkDbFunctionsExtensions.SetMaskLength) => NullPropagatingFunction("set_masklen",      new[] { arguments[1], arguments[2] }, arguments[1].Type, arguments[1].TypeMapping),
            nameof(KdbndpNetworkDbFunctionsExtensions.Text)          => NullPropagatingFunction("text",             new[] { arguments[1] }, typeof(string)),
            nameof(KdbndpNetworkDbFunctionsExtensions.SameFamily)    => NullPropagatingFunction("inet_same_family", new[] { arguments[1], arguments[2] }, typeof(bool)),
            nameof(KdbndpNetworkDbFunctionsExtensions.Merge)         => NullPropagatingFunction("inet_merge",       new[] { arguments[1], arguments[2] }, typeof((IPAddress Address, int Subnet)), _cidrMapping),
            nameof(KdbndpNetworkDbFunctionsExtensions.Truncate)      => NullPropagatingFunction("trunc",            new[] { arguments[1] }, typeof(PhysicalAddress), arguments[1].TypeMapping),
            nameof(KdbndpNetworkDbFunctionsExtensions.Set7BitMac8)   => NullPropagatingFunction("macaddr8_set7bit", new[] { arguments[1] }, typeof(PhysicalAddress), _macaddr8Mapping),

            _ => null
        };

        SqlFunctionExpression NullPropagatingFunction(
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
}
