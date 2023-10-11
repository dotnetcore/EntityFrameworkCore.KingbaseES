using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Kdbndp.EntityFrameworkCore.KingbaseES.Diagnostics.Internal;

public class KdbndpLoggingDefinitions : RelationalLoggingDefinitions
{
    public EventDefinitionBase? LogFoundDefaultSchema;
    public EventDefinitionBase? LogFoundColumn;
    public EventDefinitionBase? LogFoundForeignKey;
    public EventDefinitionBase? LogFoundCollation;
    public EventDefinitionBase? LogPrincipalTableNotInSelectionSet;
    public EventDefinitionBase? LogMissingSchema;
    public EventDefinitionBase? LogMissingTable;
    public EventDefinitionBase? LogFoundSequence;
    public EventDefinitionBase? LogFoundTable;
    public EventDefinitionBase? LogFoundIndex;
    public EventDefinitionBase? LogFoundPrimaryKey;
    public EventDefinitionBase? LogFoundUniqueConstraint;
    public EventDefinitionBase? LogPrincipalColumnNotFound;
    public EventDefinitionBase? LogEnumColumnSkipped;
    public EventDefinitionBase? LogExpressionIndexSkipped;
    public EventDefinitionBase? LogUnsupportedColumnConstraintSkipped;
    public EventDefinitionBase? LogUnsupportedColumnIndexSkipped;
}
