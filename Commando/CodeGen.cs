using dhll.CodeGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Commando
{

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

            file.WriteLine($"[CommandDef({allProps})]");
            file.WriteLine($"public class {fromDef.Name}");
            file.OpenBlock(true);
            // file.WriteLine("// wow!");

            // This is where the different options would go......
            // NOTE: I don't think that we need to fiddle with extra attributes, etc. on the command defs.
            // We already have a way to define them, and their rules can be interpreted when the 
            // command line is first processed...  Having a concrete type is a nice to have.?
            // --> Trying to decide where to draw the line.... This is all stuff that I need to think about....

            foreach (var op in fromDef.Options)
            {
                string typeName = op.DataType.Name;
                string val = op.DefaultValue;
                
                if (op.DataType == typeof(string)){
                if (val == string.Empty) { val = "\"\""; } else if (val == null) { val = "null"; }
                }
                
                string opProps = $"Name = {op.Name}";
                if (op.IsRequired)
                {
                    opProps += $", Required = true";
                }
                if (!string.IsNullOrWhiteSpace(op.HelpText)) { 
                    opProps += $", HelpText = {op.HelpText}";
                }
                file.WriteLine($"[CommandOption({opProps})]");
                file.WriteLine($"public {typeName} {op.Name} = {val};");

                file.NextLine();
            }

            file.CloseBlock();

            file.Save(toPath);

            Log.Info($"Wrote C# code to file: {toPath}");
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
