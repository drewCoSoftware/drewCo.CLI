using drewCo.Tools.Logging;
using Tommy;

namespace drewCo.CLI;

// ======================================================================================================================================================
public class DefGenerator
{

  // ------------------------------------------------------------------------------------------
  public CommandDef[] ParseCommandDefsFromTOML(string path)
  {
    var res = new List<CommandDef>();

    Log.Info("Parsing command defs from file...");
    if (!File.Exists(path))
    {
      throw new FileNotFoundException($"The file at path: {path} does not exist!");
    }

    using (var reader = File.OpenText(path))
    {
      TomlTable table = TOML.Parse(reader);

      // TomlTable interface is kind of jank.. over abstracted, but let's see if we can pull this thing apart...
      var allKeys = table.Keys.ToArray();

      foreach (var k in allKeys)
      {
        var t = table[k];
        if (!t.IsTable)
        {
          throw new InvalidOperationException("There must be exactly one table in the file!");
        }

        string comment = t.Comment;

        // NOTE: Command defs don't use constraints:
        var txtc1 = Helpers.ParseTOMLComment(t.Comment, true);
        var def = new CommandDef()
        {
          Name = k,
          HelpText = txtc1.Text,
          Alias = txtc1.Constraints.Aliases?.FirstOrDefault()
        };

        // Each of the children will then be their own property (option) on the def:
        var cKeys = t.Keys.ToArray();
        foreach (var ck in cKeys)
        {
          TomlNode child = t[ck];
          var txtc2 = Helpers.ParseTOMLComment(child.Comment, false);

          var op = new CommandOption();
          op.Name = ck;
          op.DataType = Helpers.GetDataType(child);
          op.HelpText = txtc2.Text;
          op.IsRequired = txtc2.Constraints.IsRequired;
          op.Options = txtc2.Constraints.Options;
          op.Aliases = txtc2.Constraints.Aliases;
          op.DefaultValue = child.HasValue ? child.ToString() : null;
          def.Options.Add(op);
        }

        res.Add(def);

      }

      return res.ToArray();

    }
  }
}
