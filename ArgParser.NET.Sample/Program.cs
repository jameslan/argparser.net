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
            var options = new OptionSet(ShortHelp) {
                {'h', "help", "print this message", ShortHelp},
                {"server", "server address", (string v) => _server = v},
                {'u', "user", "user name", (string v) => _username = v},
                {"help", "print detailed help message", new OptionSet(HelpUsage) {
                    {"commands", "list all subcommands", new OptionSet(ListSubCommands)}
                }},
                {"checkout", "Checkout files", new OptionSet(Checkout) {
                    {'f', "force", "force checkout", () => _forceCheckout = true}
                }},
                {"checkin", "Checkin files", new OptionSet(Checkin) {
                    {'a', "all", "checkin all files", () => _checkinAll = true}
                }}
            };
            _extra = options.Parse(args);
        }

        private static void Checkin() {
            throw new NotImplementedException();
        }

        private static void Checkout() {
            throw new NotImplementedException();
        }

        private static void ListSubCommands() {
            throw new NotImplementedException();
        }

        private static void HelpUsage() {
            throw new NotImplementedException();
        }

        private static void ShortHelp() {
            throw new NotImplementedException();
        }
    }
}
