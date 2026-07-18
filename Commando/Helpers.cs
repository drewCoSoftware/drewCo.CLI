using Tommy;

namespace Commando
{
    // =========================================================================================================================
    internal static class Helpers
    {

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
                        string[] vals = l.Substring("@OPTIONS".Length).Split('|');
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
