using dhll.CodeGen;
using drewCo.Tools;
using drewCo.Tools.Logging;
using System.Net.Http.Headers;
using System.Text;

namespace drewCo.CLI
{

  // ==================================================================================================
  public class CodeGen
  {
    // --------------------------------------------------------------------------------
    public void OutputCSharp(CommandDef def, string toPath, string? useNamespace = null)
    {
      OutputCSharp(new[] { def }, toPath, useNamespace);
    }

    // --------------------------------------------------------------------------------
    public void OutputCSharp(IList<CommandDef> fromDefs, string toPath, string? useNamespace = null)
    {
      var file = new CodeFile();

      file.WriteLine("// GENERATED CODE!  DO NOT EDIT BY HAND!");
      file.WriteLine("using Tommy;");
      file.WriteLine("using drewCo.CLI;");

      using (var fs = File.Open(toPath, FileMode.Truncate))
      {
        foreach (var def in fromDefs)
        {
          OutputCSharp(def, file, useNamespace);
        }

        string output = file.ToString();
        var bytes = Encoding.UTF8.GetBytes(output + Environment.NewLine);
        fs.Write(bytes, 0, bytes.Length);
      }
    }

    // --------------------------------------------------------------------------------
    /// <summary>
    /// Format the help text so that it is on a single line.
    /// </summary>
    private string FormatHelpText(string input)
    {
      string res = input.Replace("\r", "").Replace("\n", " ");
      return res;
    }

    // --------------------------------------------------------------------------------
    public void OutputCSharp(CommandDef fromDef, CodeFile file, string? useNamespace = null)
    {

      string nameProp = $"Name = \"{fromDef.Name}\"";
      var allProps = nameProp;
      if (!string.IsNullOrWhiteSpace(fromDef.HelpText))
      {
        allProps += $", HelpText = \"{FormatHelpText(fromDef.HelpText)}\"";
      }

      file.NextLine();

      if (!string.IsNullOrWhiteSpace(useNamespace))
      {
        file.WriteLine($"namespace {useNamespace};");
        file.NextLine();
      }

      file.WriteLine($"public class {fromDef.Name} : {nameof(ICommand)}");
      file.OpenBlock(true);

      // This is where the different options would go......
      // NOTE: I don't think that we need to fiddle with extra attributes, etc. on the command defs.
      // We already have a way to define them, and their rules can be interpreted when the 
      // command line is first processed...  Having a concrete type is a nice to have.?
      // --> Trying to decide where to draw the line.... This is all stuff that I need to think about....

      // Property definitions.
      foreach (var op in fromDef.Options)
      {
        string typeName = ConvertTypeName(op.DataType);
        string val = op.DefaultValue;

        val = FormatValue(op, val);

        string opProps = $"Name = {op.Name}";
        if (op.IsRequired)
        {
          opProps += $", Required = true";
        }
        if (!string.IsNullOrWhiteSpace(op.HelpText))
        {
          opProps += $", HelpText = {FormatHelpText(op.HelpText)}";
        }

        file.WriteLine($"public {typeName} {op.Name} = {val};");
      }

      // Default constructor, for correct code elsewhere....
      file.NextLine();
      file.WriteLine($"public {fromDef.Name}() {{ }}");


      // A function that will deserialize this against a TomlTable instance.
      file.NextLine();
      file.WriteLine($"public static {fromDef.Name} FromToml(TomlTable table)");
      file.OpenBlock(true);

      file.WriteLine($"var res = new {fromDef.Name}();");

      foreach (var item in fromDef.Options)
      {
        string getfuncName = GetValueByType(item.DataType);
        string useName = item.Name;
        string defaultValueArg = GetDefaultValuesString(item);

        string getValueCall = $"{getfuncName}(\"{useName}\", {defaultValueArg})";
        file.WriteLine($"res.{item.Name} = table.{getValueCall};");
      }
      file.WriteLine("return res;");
      file.CloseBlock(1);

      EmitValidationFunction(fromDef, file);

      EmitConfigureFunction(fromDef, file);

      file.CloseBlock(1);

    }

    // --------------------------------------------------------------------------------------------------------------------------
    private string ConvertTypeName(Type t)
    {
      // TODO: We can come up with better type names yet, something that is more native the C# experience.....
      string res = t.Name;
      if (t.IsArray)
      {
        res = ConvertTypeName(t.GetElementType()) + "[]"; 
      }
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tells us if the input string is quoted or not.
    /// </summary>
    [Obsolete("Replace with drewco.tools version > 1.5.1.1")]
    public static bool IsQuoted(string input)
    {
      string test = input.Trim();
      bool res = test.StartsWith("\"") && test.EndsWith("\"");
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static string FormatValue(CommandOption op, string val)
    {
      if (op.DataType == typeof(string))
      {
        if (val == string.Empty) { val = "string.Empty"; }
        else if (val == null) { val = "null"; }
        else
        {
          if (!IsQuoted(val))
          {
            val = StringTools.Quote(val);
          }
          return val;
        }
      }
      if (op.DataType == typeof(bool))
      {
        val = val.ToLower();
      }
      if (op.DataType == typeof(float))
      {
        val += "f";
      }
      if (op.DataType == typeof(double))
      {
        val += "d";
      }

      return val;
    }

    // --------------------------------------------------------------------------------
    public static string GetDefaultValuesString(CommandOption op)
    {
      string res = FormatValue(op, op.DefaultValue);
      return res;
    }

    // --------------------------------------------------------------------------------
    /// <summary>
    /// Configuration function so that we can setup the command for use in your program.
    /// </summary>
    private void EmitConfigureFunction(CommandDef def, CodeFile file)
    {
      file.NextLine();

      file.WriteLine($"public CommandDef Configure()");
      file.OpenBlock(true);

      file.WriteLine($"var res = new CommandDef();");
      file.WriteLine($"res.Name = \"{def.Name}\";");
      if (def.Alias != null)
      {
        file.WriteLine($"res.Alias = \"{def.Alias}\";");
      }
      file.WriteLine($"res.HelpText = \"{FormatHelpText(def.HelpText)}\";");
      foreach (var op in def.Options)
      {
        file.NextLine();
        var opName = $"{StringTools.LowerFirst(op.Name)}Option";
        file.WriteLine($"var {opName} = new CommandOption();");
        file.WriteLine($"{opName}.Name = \"{op.Name}\";");
        file.WriteLine($"{opName}.HelpText = \"{FormatHelpText(op.HelpText)}\";");
        file.WriteLine($"{opName}.DataType = typeof({ConvertTypeName(op.DataType)});");
        file.WriteLine($"{opName}.IsRequired = {(op.IsRequired ? "true" : "false")};");

        if (op.Aliases != null)
        {
          file.WriteLine($"{opName}.Aliases = new[] {{ {string.Join(", ", from x in op.Aliases select "\"" + x + "\"")} }};");
        }

        string useOpsVal = "null";
        if (op.Options != null)
        {
          useOpsVal = $"new[] {{ " + string.Join(", ", from x in op.Options select $"\"{x}\"") + " }";
        }
        file.WriteLine($"{opName}.Options = {useOpsVal};");
        file.WriteLine($"res.Options.Add({opName});");
      }

      file.NextLine();
      file.WriteLine("return res;");

      file.CloseBlock(1);
    }

    // --------------------------------------------------------------------------------
    private void EmitValidationFunction(CommandDef def, CodeFile file)
    {
      file.NextLine();
      file.WriteLine($"public {nameof(CommandValidationResult)} Validate()");
      file.OpenBlock(true);

      file.WriteLine($"var res = new {nameof(CommandValidationResult)}();");

      foreach (var item in def.Options)
      {
        // Check the constraints.....
        if (item.IsRequired)
        {
          if (item.DataType == typeof(string))
          {
            file.WriteLine($"if (string.IsNullOrWhiteSpace({item.Name}))");
            file.OpenBlock(true);

            file.WriteLine($"res.AddError(\"Option: '{item.Name}' ({item.GetCLIName()}) is required!\");");

            file.CloseBlock(1);
          }
          else
          {
            // Other types should be OK?
            if (item.DefaultValue == null)
            {
              throw new NotImplementedException("not there yet...");
            }
          }
        }

        // Make sure that the supplied value exists in the list of possible options.
        if (item.Options != null)
        {
        }
      }

      file.WriteLine("return res;");
      file.CloseBlock(1);
    }

    // --------------------------------------------------------------------------------
    public string GetValueByType(Type fromType)
    {
      if (fromType == typeof(string))
      {
        return "GetString";
      }
      else if (fromType == typeof(int))
      {
        return "GetInt";
      }
      else if (fromType == typeof(long))
      {
        return "GetLong";
      }
      else if (fromType == typeof(bool))
      {
        return "GetBool";
      }
      else if (fromType == typeof(float))
      {
        return "GetSingle";
      }
      else if (fromType == typeof(double))
      {
        return "GetDouble";
      }
      else if (fromType.IsArray)
      {
        var eType = fromType.GetElementType();
        if (eType == typeof(string))
        {
          return "GetStringArray";
        }
        else if (eType == typeof(int))
        {
          return "GetIntArray";
        }
        else
        {
          throw new InvalidOperationException("Unsupported array type!");
        }
      }

      // Other cases here.....
      else
      {
        throw new ArgumentOutOfRangeException($"Unsupported type: {fromType}!");
      }
    }

    // --------------------------------------------------------------------------------
    /// <summary>
    /// Bandaid function to get over odd Tommy API...
    /// </summary>
    public string AsType(Type fromType)
    {
      if (fromType == typeof(string))
      {
        return "AsString";
      }
      else if (fromType == typeof(int))
      {
        return "AsInteger";
      }
      // Other cases here.....
      else
      {
        throw new ArgumentOutOfRangeException($"Unsupported type: {fromType}!");
      }
    }

  }
}
