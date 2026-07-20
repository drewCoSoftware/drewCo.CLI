using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Tommy;

namespace Commando;

// ======================================================================================================================================================
public class DefGenerator
    {

        // ------------------------------------------------------------------------------------------
        public CommandDef[] ParseCommandDefsFromTOML(string path)
        {
            Log.Info("Parsing command defs from file...");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"The file at path: {path} does not exist!");
            }

            var res = new List<CommandDef>();


            using (var reader = File.OpenText(path))
            {
                TomlTable table = TOML.Parse(reader);

                // TomlTable interface is kind of jank.. over abstracted, but let's see if we can pull this thing apart...
                var allKeys = table.Keys.ToArray();

                Log.Info($"there are: {allKeys.Length} defined commands.");

                // NOTE: Multi-line comments appear to be supported.
                foreach (var k in allKeys)
                {
                    var t = table[k];

                    string comment = t.Comment;

                    Log.Info($"The help text is: {comment ?? "<null>"}");

                    // NOTE: Command defs don't use constraints:
                    var txtc1 = Helpers.ParseTOMLComment(t.Comment);
                    var def = new CommandDef()
                    {
                        Name = k,
                        HelpText = txtc1.Text,
                    };

                    // Each of the children will then be their own property (option) on the def:
                    var cKeys = t.Keys.ToArray();
                    foreach (var ck in cKeys)
                    {
                        TomlNode child = t[ck];
                        var txtc2 = Helpers.ParseTOMLComment(child.Comment);

                        var op = new CommandOption();
                        op.Name = ck;
                        op.DataType = Helpers.GetDataType(child);
                        op.HelpText = txtc2.Text;
                        op.IsRequired = txtc2.Constraints.IsRequired;
                        op.Options = txtc2.Constraints.Options;
                        op.DefaultValue = child.HasValue ? child.ToString()  : null;
                        def.Options.Add(op);
                    }

                    res.Add(def);
                }
            }

            return res.ToArray();
        }
    }
