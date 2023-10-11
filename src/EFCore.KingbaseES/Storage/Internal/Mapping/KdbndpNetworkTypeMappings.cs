using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

/// <summary>
/// The type mapping for the KingbaseES macaddr type.
/// </summary>
/// <remarks>
/// See: https://www.KingbaseES.org/docs/current/static/datatype-net-types.html#DATATYPE-MACADDR
/// </remarks>
public class KdbndpMacaddrTypeMapping : KdbndpTypeMapping
{
    /// <summary>
    /// Constructs an instance of the <see cref="KdbndpMacaddrTypeMapping"/> class.
    /// </summary>
    public KdbndpMacaddrTypeMapping() : base("macaddr", typeof(PhysicalAddress), KdbndpDbType.MacAddr) {}

    protected KdbndpMacaddrTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.MacAddr) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpMacaddrTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"MACADDR '{(PhysicalAddress)value}'";

    public override Expression GenerateCodeLiteral(object value)
        => Expression.Call(ParseMethod, Expression.Constant(((PhysicalAddress)value).ToString()));

    private static readonly MethodInfo ParseMethod = typeof(PhysicalAddress).GetMethod("Parse", new[] { typeof(string) })!;
}

/// <summary>
/// The type mapping for the KingbaseES macaddr8 type.
/// </summary>
/// <remarks>
/// See: https://www.KingbaseES.org/docs/current/static/datatype-net-types.html#DATATYPE-MACADDR8
/// </remarks>
public class KdbndpMacaddr8TypeMapping : KdbndpTypeMapping
{
    /// <summary>
    /// Constructs an instance of the <see cref="KdbndpMacaddr8TypeMapping"/> class.
    /// </summary>
    public KdbndpMacaddr8TypeMapping() : base("macaddr8", typeof(PhysicalAddress), KdbndpDbType.MacAddr8) {}

    protected KdbndpMacaddr8TypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.MacAddr8) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpMacaddr8TypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"MACADDR8 '{(PhysicalAddress)value}'";

    public override Expression GenerateCodeLiteral(object value)
        => Expression.Call(ParseMethod, Expression.Constant(((PhysicalAddress)value).ToString()));

    private static readonly MethodInfo ParseMethod = typeof(PhysicalAddress).GetMethod("Parse", new[] { typeof(string) })!;
}

/// <summary>
/// The type mapping for the KingbaseES inet type.
/// </summary>
/// <remarks>
/// See: https://www.KingbaseES.org/docs/current/static/datatype-net-types.html#DATATYPE-INET
/// </remarks>
public class KdbndpInetTypeMapping : KdbndpTypeMapping
{
    /// <summary>
    /// Constructs an instance of the <see cref="KdbndpInetTypeMapping"/> class.
    /// </summary>
    public KdbndpInetTypeMapping() : base("inet", typeof(IPAddress), KdbndpDbType.Inet) {}

    protected KdbndpInetTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Inet) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpInetTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
        => $"INET '{(IPAddress)value}'";

    public override Expression GenerateCodeLiteral(object value)
        => Expression.Call(ParseMethod, Expression.Constant(((IPAddress)value).ToString()));

    private static readonly MethodInfo ParseMethod = typeof(IPAddress).GetMethod("Parse", new[] { typeof(string) })!;
}

/// <summary>
/// The type mapping for the KingbaseES cidr type.
/// </summary>
/// <remarks>
/// See: https://www.KingbaseES.org/docs/current/static/datatype-net-types.html#DATATYPE-CIDR
/// </remarks>
public class KdbndpCidrTypeMapping : KdbndpTypeMapping
{
    /// <summary>
    /// Constructs an instance of the <see cref="KdbndpCidrTypeMapping"/> class.
    /// </summary>
    public KdbndpCidrTypeMapping() : base("cidr", typeof((IPAddress, int)), KdbndpDbType.Cidr) {}

    protected KdbndpCidrTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Cidr) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpCidrTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var cidr = ((IPAddress Address, int Subnet))value;
        return $"CIDR '{cidr.Address}/{cidr.Subnet}'";
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        var cidr = ((IPAddress Address, int Subnet))value;
        return Expression.New(
            Constructor,
            Expression.Call(ParseMethod, Expression.Constant(cidr.Address.ToString())),
            Expression.Constant(cidr.Subnet));
    }

    private static readonly MethodInfo ParseMethod = typeof(IPAddress).GetMethod("Parse", new[] { typeof(string) })!;

    private static readonly ConstructorInfo Constructor =
        typeof((IPAddress, int)).GetConstructor(new[] { typeof(IPAddress), typeof(int) })!;
}
