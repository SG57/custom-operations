using System.Text.Json.Serialization;
using System.Web;

namespace Custom_Operations;

public class OperationItem
{
    public int Index { get; set; }
    public string? Name { get; set; }
    public OperationType Type { get; set; }
    public object? Arguments { get; set; }

    [JsonIgnore] public string DisplayName => Name ?? "...";

    public static readonly OperationItem EditConfig = new()
    {
        Name = "Edit Configuration"
        , Type = OperationType.StartProcess
        , Arguments = $"[\"{HttpUtility.JavaScriptStringEncode(CustomOperationsConfiguration.ConfigFile)}\"]"
    };
}
