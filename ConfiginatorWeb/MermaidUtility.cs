using System.Text;
using Allard.Json;

namespace ConfiginatorWeb;

public static class MermaidUtility
{
    /// <summary>
    /// Create the mermaid js diagram for the variable set hierarchy.
    /// </summary>
    /// <param name="variableSetComposed"></param>
    /// <param name="selectedNode"></param>
    /// <returns></returns>
    public static string FlowChartForVariableSet(VariableSetComposed variableSetComposed, string selectedNode)
    {
        var mermaid = new StringBuilder()
            .AppendLine("graph BT")
            .AppendLine("classDef selected fill:#f9f,stroke:#333,stroke-width:4px");

        AddChildren(variableSetComposed.Root);
        
        // highlight the selected node
        mermaid.AppendLine($"style {selectedNode} fill:#00758f");
        return mermaid.ToString();
        
        void AddChildren(VariableSetComposed parent)
        {
            foreach (var child in parent.Children)
            {
                mermaid.AppendLine($"{child.VariableSetName} --> {parent.VariableSetName}");
                AddChildren(child);
            }

            // click event for each node
            mermaid.AppendLine($"click {parent.VariableSetName} \"/Variable?variableSetName={parent.VariableSetName}\" \" \"");
        }
    }
}
