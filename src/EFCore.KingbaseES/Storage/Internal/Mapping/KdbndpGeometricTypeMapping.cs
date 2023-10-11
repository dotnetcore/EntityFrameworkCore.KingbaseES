using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpPointTypeMapping : KdbndpTypeMapping
{
    public KdbndpPointTypeMapping() : base("point", typeof(KdbndpPoint), KdbndpDbType.Point) {}

    protected KdbndpPointTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Point) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpPointTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var point = (KdbndpPoint)value;
        return FormattableString.Invariant($"POINT '({point.X:G17},{point.Y:G17})'");
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        var point = (KdbndpPoint)value;
        return Expression.New(Constructor, Expression.Constant(point.X), Expression.Constant(point.Y));
    }

    private static readonly ConstructorInfo Constructor =
        typeof(KdbndpPoint).GetConstructor(new[] { typeof(double), typeof(double) })!;
}

public class KdbndpLineTypeMapping : KdbndpTypeMapping
{
    public KdbndpLineTypeMapping() : base("line", typeof(KdbndpLine), KdbndpDbType.Line) {}

    protected KdbndpLineTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Line) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpLineTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var line = (KdbndpLine)value;
        var a = line.A.ToString("G17", CultureInfo.InvariantCulture);
        var b = line.B.ToString("G17", CultureInfo.InvariantCulture);
        var c = line.C.ToString("G17", CultureInfo.InvariantCulture);
        return $"LINE '{{{a},{b},{c}}}'";
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        var line = (KdbndpLine)value;
        return Expression.New(
            Constructor,
            Expression.Constant(line.A), Expression.Constant(line.B), Expression.Constant(line.C));
    }

    private static readonly ConstructorInfo Constructor =
        typeof(KdbndpLine).GetConstructor(new[] { typeof(double), typeof(double), typeof(double) })!;
}

public class KdbndpLineSegmentTypeMapping : KdbndpTypeMapping
{
    public KdbndpLineSegmentTypeMapping() : base("lseg", typeof(KdbndpLSeg), KdbndpDbType.LSeg) {}

    protected KdbndpLineSegmentTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.LSeg) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpLineSegmentTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var lseg = (KdbndpLSeg)value;
        return FormattableString.Invariant($"LSEG '[({lseg.Start.X:G17},{lseg.Start.Y:G17}),({lseg.End.X:G17},{lseg.End.Y:G17})]'");
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        var lseg = (KdbndpLSeg)value;
        return Expression.New(
            Constructor,
            Expression.Constant(lseg.Start.X), Expression.Constant(lseg.Start.Y),
            Expression.Constant(lseg.End.X), Expression.Constant(lseg.End.Y));
    }

    private static readonly ConstructorInfo Constructor =
        typeof(KdbndpLSeg).GetConstructor(new[] { typeof(double), typeof(double), typeof(double), typeof(double) })!;
}

public class KdbndpBoxTypeMapping : KdbndpTypeMapping
{
    public KdbndpBoxTypeMapping() : base("box", typeof(KdbndpBox), KdbndpDbType.Box) {}

    protected KdbndpBoxTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Box) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpBoxTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var box = (KdbndpBox)value;
        return FormattableString.Invariant($"BOX '(({box.Right:G17},{box.Top:G17}),({box.Left:G17},{box.Bottom:G17}))'");
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        var box = (KdbndpBox)value;
        return Expression.New(
            Constructor,
            Expression.Constant(box.Top), Expression.Constant(box.Right),
            Expression.Constant(box.Bottom), Expression.Constant(box.Left));
    }

    private static readonly ConstructorInfo Constructor =
        typeof(KdbndpBox).GetConstructor(new[] { typeof(double), typeof(double), typeof(double), typeof(double) })!;
}

public class KdbndpPathTypeMapping : KdbndpTypeMapping
{
    public KdbndpPathTypeMapping() : base("path", typeof(KdbndpPath), KdbndpDbType.Path) {}

    protected KdbndpPathTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Path) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpPathTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var path = (KdbndpPath)value;
        var sb = new StringBuilder();
        sb.Append("PATH '");
        sb.Append(path.Open ? '[' : '(');
        for (var i = 0; i < path.Count; i++)
        {
            sb.Append('(');
            sb.Append(path[i].X.ToString("G17", CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(path[i].Y.ToString("G17", CultureInfo.InvariantCulture));
            sb.Append(')');
            if (i < path.Count - 1)
            {
                sb.Append(',');
            }
        }
        sb.Append(path.Open ? ']' : ')');
        sb.Append('\'');
        return sb.ToString();
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        var path = (KdbndpPath)value;
        return Expression.New(
            Constructor,
            Expression.NewArrayInit(typeof(KdbndpPoint),
                path.Select(p => Expression.New(
                    PointConstructor,
                    Expression.Constant(p.X), Expression.Constant(p.Y)))),
            Expression.Constant(path.Open));
    }

    private static readonly ConstructorInfo Constructor =
        typeof(KdbndpPath).GetConstructor(new[] { typeof(IEnumerable<KdbndpPoint>), typeof(bool) })!;

    private static readonly ConstructorInfo PointConstructor =
        typeof(KdbndpPoint).GetConstructor(new[] { typeof(double), typeof(double) })!;
}

public class KdbndpPolygonTypeMapping : KdbndpTypeMapping
{
    public KdbndpPolygonTypeMapping() : base("polygon", typeof(KdbndpPolygon), KdbndpDbType.Polygon) {}

    protected KdbndpPolygonTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Polygon) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpPolygonTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var polygon = (KdbndpPolygon)value;
        var sb = new StringBuilder();
        sb.Append("POLYGON '(");
        for (var i = 0; i < polygon.Count; i++)
        {
            sb.Append('(');
            sb.Append(polygon[i].X.ToString("G17", CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(polygon[i].Y.ToString("G17", CultureInfo.InvariantCulture));
            sb.Append(')');
            if (i < polygon.Count - 1)
            {
                sb.Append(',');
            }
        }
        sb.Append(")'");
        return sb.ToString();
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        var polygon = (KdbndpPolygon)value;
        return Expression.New(
            Constructor,
            Expression.NewArrayInit(typeof(KdbndpPoint),
                polygon.Select(p => Expression.New(
                    PointConstructor,
                    Expression.Constant(p.X), Expression.Constant(p.Y)))));
    }

    private static readonly ConstructorInfo Constructor =
        typeof(KdbndpPolygon).GetConstructor(new[] { typeof(KdbndpPoint[]) })!;

    private static readonly ConstructorInfo PointConstructor =
        typeof(KdbndpPoint).GetConstructor(new[] { typeof(double), typeof(double) })!;
}

public class KdbndpCircleTypeMapping : KdbndpTypeMapping
{
    public KdbndpCircleTypeMapping() : base("circle", typeof(KdbndpCircle), KdbndpDbType.Circle) {}

    protected KdbndpCircleTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters, KdbndpDbType.Circle) {}

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpCircleTypeMapping(parameters);

    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var circle = (KdbndpCircle)value;
        return FormattableString.Invariant($"CIRCLE '<({circle.X:G17},{circle.Y:G17}),{circle.Radius:G17}>'");
    }

    public override Expression GenerateCodeLiteral(object value)
    {
        var circle = (KdbndpCircle)value;
        return Expression.New(
            Constructor,
            Expression.Constant(circle.X), Expression.Constant(circle.Y), Expression.Constant(circle.Radius));
    }

    private static readonly ConstructorInfo Constructor =
        typeof(KdbndpCircle).GetConstructor(new[] { typeof(double), typeof(double), typeof(double) })!;
}
