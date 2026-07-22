using Tommy;

namespace Commando
{
  // =========================================================================================================================
  public static class Helpers
  {

    /// <summary>
    /// Get the TOML table from the given file path.
    /// </summary>
    /// <remarks>
    /// This function expects that there will be one, single table in
    /// the TOML file that represents the command, otherwise it will throw an exception.
    /// </remarks>
    /// <param name="expectedName">Optional, expected command name.  If <see cref="commandName"/> does not match, and exception will be thrown! </param>
    public static TomlTable FromFile(string path, out string commandName, string? expectedName = null)
    {
      using (var reader = File.OpenText(path))
      {
        TomlTable table = TOML.Parse(reader);

        var keyCount = table.Keys.Count();
        if (keyCount == 0 || keyCount > 1)
        {
          throw new InvalidOperationException("Invalid number of keys!");
        }

        var key = table.Keys.ElementAt(0);
        commandName = key;
        if (expectedName != null && commandName != expectedName)
        {
          throw new InvalidOperationException($"Expecting command name: {expectedName} but got: {commandName} instead!");
        }

        var res = table[key].AsTable;
        if (res == null)
        {
          throw new InvalidOperationException("Invalid table!");
        }
        return res;
      }
    }

    // ---------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The constraints are embedded into the TOML comments, starting
    /// on each new line, and they will be stripped out of the overall comment...
    /// </summary>
    internal static (string Text, Constraints Constraints) ParseTOMLComment(string input)
    {

      Constraints c = new Constraints();
      List<string> text = new List<string>();

      if (!string.IsNullOrWhiteSpace(input))
      {
        var lines = input.Split(new[] { '\n' });
        foreach (var l in lines)
        {
          var useLine = l.Trim();
          if (l.StartsWith("@REQUIRED"))
          {
            // NOTE: The rest of the line is ignored on purpose.
            c.IsRequired = true;
          }
          else if (l.StartsWith("@OPTIONS"))
          {
            // This command has a set of valid options.
            string[] vals = l.Substring("@OPTIONS".Length).Split(',');
            c.Options = (from x in vals select x.Trim()).ToArray();
          }
          else
          {
            text.Add(useLine);
          }
        }
      }
      var res = (string.Join(Environment.NewLine, text), c);
      return res;
    }


    // ---------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Yep, doing the heavy lifting for them.....
    /// </summary>
    /// <param name="fromNode"></param>
    /// <returns></returns>
    public static Type GetDataType(TomlNode fromNode)
    {
      if (fromNode.IsString) { return typeof(string); }
      if (fromNode.IsInteger) { return typeof(int); }
      if (fromNode.IsFloat) { return typeof(float); }
      if (fromNode.IsDateTimeOffset) { return typeof(DateTimeOffset); }
      if (fromNode.IsDateTime) { return typeof(DateTime); }
      if (fromNode.IsBoolean) { return typeof(bool); }

      // NOTE: We can add array support or whatever later.
      // The child nodes will have to be evaluated, and will have to all be of the same type tho.

      throw new InvalidOperationException("Could not determine a supported type for this node!");
    }
  }

}
