using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commando
{
    /// <summary>
    /// Place this on a class to make it known that it is a CLI command.
    /// </summary>
    public class CommandAttribute
    {
        public CommandAttribute(string name_) { Name = name_; }
        public string Name { get; private set; }
    }
}
