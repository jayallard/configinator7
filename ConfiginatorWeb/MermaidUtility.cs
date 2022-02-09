using System.Text;
using Allard.Json;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures;

public static class MermaidUtility
{
    /// <summary>
    /// Create the mermaid js diagram for the token set hierarchy.
    /// </summary>
    /// <param name="tokenSetComposed"></param>
    /// <param name="selectedNode"></param>
    /// <returns></returns>
    public static string FlowChartForTokenSet(TokenSetComposed3 tokenSetComposed, string selectedNode)
    {
        var mermaid = new StringBuilder()
            .AppendLine("graph BT")
            .AppendLine("classDef selected fill:#f9f,stroke:#333,stroke-width:4px");

        AddChildren(tokenSetComposed.Root);
        
        // highlight the selected node
        mermaid.AppendLine($"style {selectedNode} fill:#00758f");
        return mermaid.ToString();
        
        void AddChildren(TokenSetComposed3 parent)
        {
            foreach (var child in parent.Children)
            {
                mermaid.AppendLine($"{child.TokenSetName} --> {parent.TokenSetName}");
                AddChildren(child);
            }

            // click event for each node
            mermaid.AppendLine($"click {parent.TokenSetName} \"/Token?tokenSetName={parent.TokenSetName}\" \" \"");
        }
    }
}
