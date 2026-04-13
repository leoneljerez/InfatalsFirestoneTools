namespace InfatalsFirestoneTools.Models;

public sealed record PatchNote(string Version, DateOnly Date, string[] Changes);

public static class PatchNotes
{
    /// <summary>
    /// Add a new entry here with every release.
    /// The first entry is always the current version shown in the footer.
    /// </summary>
    public static readonly IReadOnlyList<PatchNote> All =
    [
        new("1.1.4", new DateOnly(2026, 4, 13),
        [
            "Add max blueprint for current machine level",
            "Add max rarity for current machine level",
            "Change order of mercenaries to align with wiki",
        ]),
        new("1.1.3", new DateOnly(2026, 4, 9),
        [
            "Add locale formatting for Date in Patch Notes",
            "Add locale formatting for numbers in Results",
        ]),
        new("1.1.2", new DateOnly(2026, 4, 9),
        [
            "More efficient sorting in Machine and Hero tab",
            "Added upgrade suggestion feature from old website",
            "Minor cleaning and improvements",
        ]),
        new("1.1.1", new DateOnly(2026, 3, 27),
        [
            "Added bulk edit view for Machines and Heroes tabs",
            "Added sort and filter to Machine and Hero lists",
            "Added Strange Dust Calculator for Guardians",
            "Optimizer settings moved to dedicated Settings page",
            "Hero scoring weights now persist to your profile",
        ]),
        new("1.0.0", new DateOnly(2026, 1, 1),
        [
            "Initial release",
            "Campaign and Arena optimizer with WASM engine",
            "Profile system with IndexedDB persistence",
            "Save/load support with legacy import",
        ]),
    ];

    public static PatchNote Current => All[0];
}