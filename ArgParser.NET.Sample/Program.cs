using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArgParser.NET.Sample
{
    class Program
    {
        static void Main(string[] args) {
            bool generalHelp;
            bool help;
            bool quick;
            var options = new Options {
                {'h', "help", "print help message", () => generalHelp = true},
                {"create", "", () => help = true, new Options {
                    {'q', "quick", "quick mode", () => quick = true}
                }}
            };
            var extra = options.Parse(args);
        }
    }
}
