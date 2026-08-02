using System.Net.WebSockets;
using Tommy;

namespace drewCo.CLI
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
    internal static (string Text, CommandOptions Options) ParseTOMLComment(string input, bool isCommandDef)
    {

      CommandOptions c = new CommandOptions();
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
          else if (l.StartsWith("@TYPE"))
          {
            string val = l.Substring("@TYPE".Length).Trim();
            c.TypeHint = val;
          }
          else if (l.StartsWith("@OPTIONS"))
          {
            // This command has a set of valid options.
            string[] vals = l.Substring("@OPTIONS".Length).Split(',');
            c.Options = (from x in vals select x.Trim()).ToArray();
          }
          else if (l.StartsWith("@ALIAS"))
          {
            // There are some aliases for this command:
            string[] aliasParts = l.Substring("@ALIAS".Length).Split(',');
            c.Aliases = (from x in aliasParts select x.Trim()).ToArray();

            // Validate that there is at most one short, and one long alias.
            int shortCount = 0;
            int longCount = 0;
            int otherCount = 0;
            foreach (var item in c.Aliases)
            {
              if (item.StartsWith("--")) { ++longCount; continue; }
              if (item.StartsWith("-")) { ++shortCount; continue; }
              else { otherCount++; }
            }

            // NOTE: otherCount == 1 SHOULD be valid for the command names!
            if (isCommandDef)
            {
              if (shortCount > 0 || longCount > 0 || otherCount < 1)
              {
                throw new InvalidOperationException($"Invalid alias specification: '{string.Join(", ", c.Aliases)}' for command def!  Use the form: <alias>");
              }
            }
            else
            {
              if (shortCount > 1 || longCount > 1 || otherCount > 0)
              {
                throw new InvalidOperationException($"Invalid alias specification: '{string.Join(", ", c.Aliases)}' for command option!  Use the form --<alias> | --<alias>, -<a>");
              }
            }


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
    public static Type GetDataType(TomlNode fromNode, CommandOptions option)
    {
      if (option.TypeHint != null)
      {
        var h = new TypeHint(option.TypeHint);
        var t = h.AsType();
        return t;
      }

      if (fromNode.IsString) { return typeof(string); }
      if (fromNode.IsInteger) { return typeof(int); }
      if (fromNode.IsFloat) { return typeof(float); }
      if (fromNode.IsDateTimeOffset) { return typeof(DateTimeOffset); }
      if (fromNode.IsDateTime) { return typeof(DateTime); }
      if (fromNode.IsBoolean) { return typeof(bool); }

      // For our arrays, all children must be of the same type....
      if (fromNode.IsArray)
      {
        Type arrayType = ResolveArrayType(fromNode, option);

        var res = arrayType.MakeArrayType();
        return res;
      }

      if (fromNode.IsTable)
      {
        throw new NotSupportedException("We don't support nested tables yet!");
      }

      // NOTE: We can add array support or whatever later.
      // The child nodes will have to be evaluated, and will have to all be of the same type tho.

      throw new InvalidOperationException("Could not determine a supported type for this node!");
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static Type ResolveArrayType(TomlNode fromNode, CommandOptions option)
    {
      if (option.TypeHint != null)
      {
        var h = new TypeHint(option.TypeHint);
        var t = h.AsType();
        return t.GetElementType();
      }

      var allTypes = new List<Type>();
      var kids = fromNode.AsArray.Children;
      foreach (var item in kids)
      {
        allTypes.Add(GetDataType(item, option));
      }
      allTypes = allTypes.Distinct().ToList();


      Type arrayType = allTypes[0];
      if (allTypes.Count > 1)
      {
        throw new InvalidOperationException("Mixed type arrays are not supported at this time!");
        arrayType = typeof(object);
      }

      return arrayType;
    }
  }

}
