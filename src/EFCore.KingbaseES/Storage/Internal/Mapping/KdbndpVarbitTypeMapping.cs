using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpVarbitTypeMapping : KdbndpTypeMapping
{
    public KdbndpVarbitTypeMapping() : base("bit varying", typeof(BitArray), KdbndpDbType.Varbit) {}

    protected KdbndpVarbitTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Varbit) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpVarbitTypeMapping(parameters);

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

        return Expression.New(Constructor,
            Expression.NewArrayInit(typeof(bool), exprs));
    }

    private static readonly ConstructorInfo Constructor =
        typeof(BitArray).GetConstructor(new[] { typeof(bool[]) })!;
}
