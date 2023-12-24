using System.Collections;
using System.Text;
using Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Json;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

/// <summary>
///     The type mapping for the KingbaseES bit string type.
/// </summary>
/// <remarks>
///     See: https://www.KingbaseES.org/docs/current/static/datatype-bit.html
/// </remarks>
public class KdbndpBitTypeMapping : KdbndpTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static KdbndpBitTypeMapping Default { get; } = new();

    /// <summary>
    ///     Constructs an instance of the <see cref="KdbndpBitTypeMapping" /> class.
    /// </summary>
    public KdbndpBitTypeMapping()
        : base("bit", typeof(BitArray), KdbndpDbType.Bit, jsonValueReaderWriter: JsonBitArrayReaderWriter.Instance)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected KdbndpBitTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Bit)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpBitTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var bits = (BitArray)value;
        var sb = new StringBuilder();
        sb.Append("B'");
        for (var i = 0; i < bits.Count; i++)
        {
            sb.Append(bits[i] ? '1' : '0');
        }

        sb.Append('\'');
        return sb.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression GenerateCodeLiteral(object value)
    {
        var bits = (BitArray)value;
        var exprs = new Expression[bits.Count];
        for (var i = 0; i < bits.Count; i++)
        {
            exprs[i] = Expression.Constant(bits[i]);
        }

        return Expression.New(Constructor, Expression.NewArrayInit(typeof(bool), exprs));
    }

    private static readonly ConstructorInfo Constructor =
        typeof(BitArray).GetConstructor(new[] { typeof(bool[]) })!;
}
