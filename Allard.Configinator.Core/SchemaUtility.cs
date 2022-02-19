using Allard.Configinator.Core.Schema;
using NJsonSchema;

namespace Allard.Configinator.Core;

public static class SchemaUtility
{
    /// <summary>
    ///     The Json Document "{}" is a valid Json Schema.
    ///     But, it's useless.
    ///     This verifies that the schema is useful.
    /// </summary>
    /// <param name="schema"></param>
    public static void ValidateRootSchema(JsonSchema schema, string schemaName)
    {
        // make sure it has at least one property.
        if (schema.Properties.Count == 0)
            throw new InvalidOperationException("The schema doesn't define any properties.");
    }

    public static void ValidateSectionReferences(SchemaInfo schema)
    {
        //schema.References.Any(r => r.)
    }
}