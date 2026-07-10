namespace CollabNotes.Application.Common;

public static class NoteBlockSplitter
{
    private const string Delimiter = "\n\n";

    public static IReadOnlyList<string> Split(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return [string.Empty];
        }

        return content.Split(Delimiter);
    }

    public static string Join(IEnumerable<string> blocks) => string.Join(Delimiter, blocks);
}
