using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using KdbndpTypes;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Storage.Internal.Mapping;

public class KdbndpEnumTypeMapping : RelationalTypeMapping
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;
    private readonly IKdbndpNameTranslator _nameTranslator;

    /// <summary>
    /// Translates the CLR member value to the KingbaseES value label.
    /// </summary>
    private readonly Dictionary<object, string> _members;

    public KdbndpEnumTypeMapping(
        string storeType,
        string? storeTypeSchema,
        Type enumType,
        ISqlGenerationHelper sqlGenerationHelper,
        IKdbndpNameTranslator? nameTranslator = null)
        : base(sqlGenerationHelper.DelimitIdentifier(storeType, storeTypeSchema), enumType)
    {
        if (!enumType.IsEnum || !enumType.IsValueType)
        {
            throw new ArgumentException($"Enum type mappings require a CLR enum. {enumType.FullName} is not an enum.");
        }

        nameTranslator ??= KdbndpConnection.GlobalTypeMapper.DefaultNameTranslator;

        _nameTranslator = nameTranslator;
        _sqlGenerationHelper = sqlGenerationHelper;
        _members = CreateValueMapping(enumType, nameTranslator);
    }

    protected KdbndpEnumTypeMapping(
        RelationalTypeMappingParameters parameters,
        ISqlGenerationHelper sqlGenerationHelper,
        IKdbndpNameTranslator nameTranslator)
        : base(parameters)
    {
        _nameTranslator = nameTranslator;
        _sqlGenerationHelper = sqlGenerationHelper;
        _members = CreateValueMapping(parameters.CoreParameters.ClrType, nameTranslator);
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new KdbndpEnumTypeMapping(parameters, _sqlGenerationHelper, _nameTranslator);

    protected override string GenerateNonNullSqlLiteral(object value) => $"'{_members[value]}'::{StoreType}";

    private static Dictionary<object, string> CreateValueMapping(Type enumType, IKdbndpNameTranslator nameTranslator)
        => enumType.GetFields(BindingFlags.Static | BindingFlags.Public)
            .ToDictionary(
                x => x.GetValue(null)!,
                x => x.GetCustomAttribute<KbNameAttribute>()?.KbName ?? nameTranslator.TranslateMemberName(x.Name));
}
