using System.Text;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Schema;
using Allard.Json;

namespace ConfiginatorWeb;

public static class MermaidUtility
{
    public static string FlowChartForSchemas(IEnumerable<SchemaDetail> schemas, SchemaName SelectedSchema)
    {
        var mermaid = new StringBuilder()
            .AppendLine("graph TD")
            .AppendLine("classDef selected fill:#f9f,stroke:#333,stroke-width:4px");


        foreach (var schema in schemas)
        {
            mermaid.AppendLine(schema.SchemaName.FullName);
            foreach (var to in schema.RefersTo)
            {
                mermaid.AppendLine($"{schema.SchemaName.FullName} --> {to.FullName}");
            }

            // foreach (var from in schema.ReferencedBy)
            // {
            //     mermaid.AppendLine($"{from.FullName} --> {schema.SchemaName.FullName}");
            // }
        }

        return mermaid.ToString();
    }
    /// <summary>
    ///     Create the mermaid js diagram for the variable set hierarchy.
    /// </summary>
    /// <param name="variableSetComposed"></param>
    /// <param name="selectedNode"></param>
    /// <returns></returns>
    public static string FlowChartForVariableSet(VariableSetComposed variableSetComposed, string selectedVariableSet)
    {
        var mermaid = new StringBuilder()
            .AppendLine("graph BT")
            .AppendLine("classDef selected fill:#f9f,stroke:#333,stroke-width:4px");

        AddChildren(variableSetComposed.Root);

        // highlight the selected node
        mermaid.AppendLine($"style {selectedVariableSet} fill:#00758f");
        return mermaid.ToString();

        void AddChildren(VariableSetComposed parent)
        {
            foreach (var child in parent.Children)
            {
                mermaid.AppendLine($"{child.VariableSetName} --> {parent.VariableSetName}");
                AddChildren(child);
            }

            // click event for each node
            mermaid.AppendLine(
                $"click {parent.VariableSetName} \"/VariableSet?variableSetName={parent.VariableSetName}\" \" \"");
        }
    }
}