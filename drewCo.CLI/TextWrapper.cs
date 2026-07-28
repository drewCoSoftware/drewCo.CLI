// CLANKER CODE:  SLOPPY SLOPPY!

using System.Text;

public static class TextWrapper
{
  public static List<string> Wrap(string message, int columnWidth, int indentWidth)
  {
    if (message == null)
    {
      throw new ArgumentNullException(nameof(message));
    }

    if (columnWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(columnWidth));
    }

    if (indentWidth < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(indentWidth));
    }

    int contentWidth = columnWidth - indentWidth;

    if (contentWidth <= 0)
    {
      throw new ArgumentException(
          "Indent width must be less than the column width.");
    }

    List<string> lines = new List<string>();
    StringBuilder currentLine = new StringBuilder();
    string indent = new string(' ', indentWidth);

    string[] words = NormalizeWhitespace(message).Split(
        ' ',
        StringSplitOptions.RemoveEmptyEntries);

    foreach (string word in words)
    {
      AddWord(lines, currentLine, indent, word, contentWidth);
    }

    if (currentLine.Length > 0)
    {
      lines.Add(indent + currentLine.ToString());
    }

    return lines;
  }

  private static void AddWord(
      List<string> lines,
      StringBuilder currentLine,
      string indent,
      string word,
      int contentWidth)
  {
    int requiredLength = currentLine.Length == 0
        ? word.Length
        : currentLine.Length + 1 + word.Length;

    if (requiredLength <= contentWidth)
    {
      if (currentLine.Length > 0)
      {
        currentLine.Append(' ');
      }

      currentLine.Append(word);
      return;
    }

    if (currentLine.Length > 0)
    {
      lines.Add(indent + currentLine.ToString());
      currentLine.Clear();
    }

    int position = 0;

    while (word.Length - position > contentWidth)
    {
      lines.Add(indent + word.Substring(position, contentWidth));
      position += contentWidth;
    }

    if (position < word.Length)
    {
      currentLine.Append(word.Substring(position));
    }
  }

  private static string NormalizeWhitespace(string text)
  {
    StringBuilder result = new StringBuilder();
    bool previousWasWhitespace = true;

    foreach (char ch in text)
    {
      if (char.IsWhiteSpace(ch))
      {
        if (!previousWasWhitespace)
        {
          result.Append(' ');
          previousWasWhitespace = true;
        }
      }
      else
      {
        result.Append(ch);
        previousWasWhitespace = false;
      }
    }

    return result.ToString().Trim();
  }
}