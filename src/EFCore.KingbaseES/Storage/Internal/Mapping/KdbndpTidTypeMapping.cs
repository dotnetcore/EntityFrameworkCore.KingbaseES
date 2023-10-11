using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpTidTypeMapping : KdbndpTypeMapping
{
    public KdbndpTidTypeMapping()
        : base("tid", typeof(KdbndpTid), KdbndpDbType.Tid) {}

    protected KdbndpTidTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Tid) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpTidTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var tid = (KdbndpTid)value;
        var builder = new StringBuilder("TID '(");
        builder.Append(tid.BlockNumber);
        builder.Append(',');
        builder.Append(tid.OffsetNumber);
        builder.Append(")'");
        return builder.ToString();
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        var tid = (KdbndpTid)value;
        return Expression.New(Constructor, Expression.Constant(tid.BlockNumber), Expression.Constant(tid.OffsetNumber));
    }

    private static readonly ConstructorInfo Constructor =
        typeof(KdbndpTid).GetConstructor(new[] { typeof(uint), typeof(ushort) })!;
}
