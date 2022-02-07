using System.Collections.ObjectModel;

namespace Allard.Configinator.Core.Schema;

public record SchemaInfo(SchemaDetail Root, ReadOnlyCollection<SchemaDetail> References);