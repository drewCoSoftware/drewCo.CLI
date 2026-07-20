using dhll.CodeGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Commando
{

    // ==================================================================================================
    public class CodeGen
    {

        // --------------------------------------------------------------------------------
        public void OutputCSharp(CommandDef fromDef, string toPath)
        {
            var file = new CodeFile();

            string nameProp = $"Name = \"{fromDef.Name}\"";
            var allProps = nameProp;
            if (!string.IsNullOrWhiteSpace(fromDef.HelpText))
            {
                allProps += $", HelpText = \"{fromDef.HelpText}\"";
            }

            file.WriteLine("// GENERATED CODE!  DO NOT EDIT BY HAND!");

            file.WriteLine("using Tommy;");
            file.NextLine();

            file.WriteLine($"public class {fromDef.Name}");
            file.OpenBlock(true);

            // This is where the different options would go......
            // NOTE: I don't think that we need to fiddle with extra attributes, etc. on the command defs.
            // We already have a way to define them, and their rules can be interpreted when the 
            // command line is first processed...  Having a concrete type is a nice to have.?
            // --> Trying to decide where to draw the line.... This is all stuff that I need to think about....

            foreach (var op in fromDef.Options)
            {
                string typeName = op.DataType.Name;
                string val = op.DefaultValue;

                if (op.DataType == typeof(string))
                {
                    if (val == string.Empty) { val = "\"\""; } else if (val == null) { val = "null"; }
                }

                string opProps = $"Name = {op.Name}";
                if (op.IsRequired)
                {
                    opProps += $", Required = true";
                }
                if (!string.IsNullOrWhiteSpace(op.HelpText))
                {
                    opProps += $", HelpText = {op.HelpText}";
                }
                //file.WriteLine($"[CommandOption({opProps})]");
                if (op.DataType == typeof(string))
                {
                    val = $"\"{val}\"";
                }
                file.WriteLine($"public {typeName} {op.Name} = {val};");
            }

            // Default constructor, for correct code elsewhere....
            file.NextLine();
            file.WriteLine($"public {fromDef.Name}() {{ }}");


            // A function that will deserialize this against a TomlTable instance.
            file.NextLine();
            file.WriteLine($"public static {fromDef.Name} FromTable(TomlTable table)");
            file.OpenBlock(true);

            file.WriteLine($"var res = new {fromDef.Name}();");

            foreach (var item in fromDef.Options)
            {
                string conv = $".{AsType(item.DataType)}.Value;";
                file.WriteLine($"res.{item.Name} = table[\"{item.Name}\"]{conv}");
            }
            file.WriteLine("return res;");
            file.CloseBlock();

            // Now a validation function.....
            file.NextLine();
            // throw new NotImplementedException("The validation function is not yet ready!");

            file.CloseBlock();

            file.Save(toPath);

            Log.Info($"Wrote C# code to file: {toPath}");
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

        //// --------------------------------------------------------------------------------
        //public void OutputCSharp(CommandDef fromDef, Stream toStream)
        //{
        //    var sb = new StringBuilder();


        //    var file = new CodeFile();
        //    file.WriteLine($"[Command(\"{fromDef.Name}\")]");

        //    file.WriteLine($"public class {fromDef.Name}");
        //    file.OpenBlock();
        //    file.WriteLine("// wow!");
        //    file.CloseBlock();

        //    file.Save(

        //    string res = sb.ToString();
        //    var data = Encoding.UTF8.GetBytes(res);
        //    toStream.Write(data, 0, data.Length);   
        //}

    }
}
