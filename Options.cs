using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSPA.upd8r
{
    public class Options
    {
        [Option('j', "json-file", Required = false, HelpText = "JSON file to output new page information to.")]
        public string JsonFile { get; set; }

        [HelpOption(HelpText = "Display this help screen.")]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("MSPA.upd8r");
            return usage.ToString();
        }
    }
}
