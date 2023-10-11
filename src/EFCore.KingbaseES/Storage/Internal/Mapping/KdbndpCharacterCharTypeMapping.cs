using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

/// <summary>
/// Type mapping for the KingbaseES 'character' data type. Handles both CLR strings and chars.
/// </summary>
/// <remarks>
/// See: https://www.KingbaseES.org/docs/current/static/datatype-character.html
/// </remarks>
public class KdbndpCharacterCharTypeMapping : CharTypeMapping, IKdbndpTypeMapping
{
    /// <inheritdoc />
    public virtual KdbndpDbType KdbndpDbType
        => KdbndpDbType.Char;

    public KdbndpCharacterCharTypeMapping(string storeType)
        : this(new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(typeof(char)),
            storeType,
            StoreTypePostfix.Size,
            System.Data.DbType.StringFixedLength,
            unicode: false,
            fixedLength: true)) {}

    protected KdbndpCharacterCharTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpCharacterCharTypeMapping(parameters);

    protected override void ConfigureParameter(DbParameter parameter)
    {
        if (parameter is not KdbndpParameter KdbndpParameter)
        {
            throw new InvalidOperationException($"Kdbndp-specific type mapping {GetType().Name} being used with non-Kdbndp parameter type {parameter.GetType().Name}");
        }

        base.ConfigureParameter(parameter);
        KdbndpParameter.KdbndpDbType = KdbndpDbType;
    }
}
