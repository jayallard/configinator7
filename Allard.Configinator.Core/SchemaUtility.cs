﻿using Allard.Configinator.Core.Model;
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
    /// <param name="schemaName"></param>
    public static void ValidateRootSchema(JsonSchema schema, string schemaName)
    {
        // make sure it has at least one property.
        if (schema.Properties.Count == 0)
            throw new InvalidOperationException("The schema doesn't define any properties. SchemaName=" + schemaName);
    }

    /// <summary>
    /// The schemas are all related to each other.
    /// Make sure they are allowed to be related to each other.
    /// IE: a schema owned by section #3 can't refer to a schema owned by section #4.
    /// </summary>
    /// <param name="relatedSchemas"></param>
    /// <param name="sectionId">Optional. If null, a global schema is being validated.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void ValidateSchemasGroup(IEnumerable<SchemaValidationProperties> relatedSchemas,
        SectionId? sectionId)
    {
        var badSchemas = sectionId == null
            // global schemas can only refer to global schemas.
            ? relatedSchemas.Where(s => !s.IsGlobal())
            
            // section schemas can refer to global, and it's own schemas of the same
            // section.
            : relatedSchemas.Where(s => !s.IsGlobal() && s.OwnerSectionId != sectionId);
        
        var bad = badSchemas.ToArray();
        if (!bad.Any()) return;
        var badMessage = string.Join(", ", badSchemas.Select(s => s.SchemaName.FullName));
        throw new InvalidOperationException(
            "The following schemas can't be used, because they don't belong to this section: " + badMessage);
    }
}

/// <summary>
/// This is the bare-bones information needed to run the validations
/// in schema utility. This is needed because sometimes we need to validate
/// things about the Schema before the schema has been loaded into an aggregate.
/// </summary>
/// <param name="SchemaName"></param>
/// <param name="OwnerSectionId"></param>
public record SchemaValidationProperties(SchemaName SchemaName, SectionId? OwnerSectionId)
{
    public bool IsGlobal() => OwnerSectionId is null;
}