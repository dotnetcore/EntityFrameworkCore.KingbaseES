using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

/// <summary>
/// The type mapping for the KingbaseES bit string type.
/// </summary>
/// <remarks>
/// See: https://www.KingbaseES.org/docs/current/static/datatype-bit.html
/// </remarks>
public class KdbndpBitTypeMapping : KdbndpTypeMapping
{
    /// <summary>
    /// Constructs an instance of the <see cref="KdbndpBitTypeMapping"/> class.
    /// </summary>
    public KdbndpBitTypeMapping() : base("bit", typeof(BitArray), KdbndpDbType.Bit) {}

    protected KdbndpBitTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Bit) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpBitTypeMapping(parameters);

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
