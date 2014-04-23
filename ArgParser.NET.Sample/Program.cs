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
            var parser = new ArgParser {};
            var extra = parser.Parse(args);
        }
    }
}
