using System;
using System.Collections.Generic;

namespace ArgParser.NET.Sample
{
    class Program
    {
        static string _server;
        static string _username;
        static bool _forceCheckout;
        static bool _checkinAll;
        static List<string> _extra;

        static void Main(string[] args) {
            var options = new CommandDef(ShortHelp) {
                {'h', "help", "print this message", ShortHelp},
                {"server", "server address", (string v) => _server = v},
                {'u', "user", "user name", (string v) => _username = v},
                {"help", "print detailed help message", new CommandDef(HelpUsage) {
                    {"commands", "list all subcommands", new CommandDef(ListSubCommands)},
                    {"options", "description of all global options", new CommandDef(ListOptions)},
                }},
                {"checkout", "Checkout files", new CommandDef(Checkout) {
                    {'f', "force", "force checkout", () => _forceCheckout = true}
                }},
                {"checkin", "Checkin files", new CommandDef(Checkin) {
                    {'a', "all", "checkin all files", () => _checkinAll = true}
                }}
            };
            _extra = options.Parse(args);
            _extra.ForEach(Console.Out.WriteLine);
            Console.Out.WriteLine(_server);
            Console.Out.WriteLine(_username);
        }

        private static void ListOptions() {
            Console.Out.WriteLine("ListOptions");
        }

        private static void Checkin() {
            Console.Out.WriteLine(_checkinAll);
            Console.Out.WriteLine("Checkin");
        }

        private static void Checkout() {
            Console.Out.WriteLine(_forceCheckout);
            Console.Out.WriteLine("Checkout");
        }

        private static void ListSubCommands() {
            Console.Out.WriteLine("ListSubCommands");
        }

        private static void HelpUsage() {
            Console.Out.WriteLine("HelpUsage");
        }

        private static void ShortHelp() {
            Console.Out.WriteLine("ShortHelp");
        }
    }
}
