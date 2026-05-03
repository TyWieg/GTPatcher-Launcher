namespace GTPatcher.Types;

/// <summary>
/// A class for a patch and its associated metadata and gameversion.
/// </summary>
public class Patch
{
    public string PatchName { get; set; } = string.Empty;
    public string PatchShorthand { get; set; } = string.Empty;
    public string PatchDescription { get; set; } = string.Empty;
    public string? PatchLink { get; set; }
    public string? GameLink { get; set; }
    public string GameName { get; set; } = "Gorilla Tag";
    public ulong? ManifestId { get; set; }
    public bool IsSteam { get; set; }
    public string? Branch { get; set; }
    public int Year { get; set; }
    public string Date { get; set; } = string.Empty;
}
