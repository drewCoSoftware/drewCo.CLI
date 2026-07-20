using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tommy;

namespace Commando
{

    // ==================================================================================================
    /// <summary>
    /// This is used when validating the commands that have been read in from a command line, file, etc.
    /// </summary>
    public class CommandValidationResult
    {
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
    }

    // ==================================================================================================
    public class ValidationError
    {
        public string Description { get; set; }
    }

    // ==================================================================================================
    public class GenerateCommand
    {
        public String Path = "example.toml";
        public String TargetLanguage = "cpp";
        public String OuputPath = "the-ouptut.cs";

        public GenerateCommand() { }

        // ------------------------------------------------------------------------------------------------
        public static GenerateCommand FromTable(TomlTable table)
        {


            var res = new GenerateCommand();
            res.Path = table["Path"].AsString.Value;
            res.TargetLanguage = table["TargetLanguage"].AsString.Value;
            res.OuputPath = table["OuputPath"].AsString.Value;
            return res;
        }

        // ------------------------------------------------------------------------------------------------
        public CommandValidationResult Validate(CommandDef againstDef)
        {
            // Match this class instance against its def!

            return new CommandValidationResult();
        }

    }
}
