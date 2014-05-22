/**
 * The MIT License (MIT)
 * 
 * Copyright (c) 2014 James Lan
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace ArgParser.NET {
    public class CommandDef : IEnumerable {
        private List<SubCommand> _subCommands = new List<SubCommand>();
        private List<Option> _options = new List<Option>();
        internal Dictionary<string, SubCommand> SubCommandsByName = new Dictionary<string, SubCommand>();
        internal Dictionary<char, Option> OptionsByShortcut = new Dictionary<char, Option>();
        internal Dictionary<string, Option> OptionsByPrototype = new Dictionary<string, Option>();

        public Action Callback { get; private set; }

        public CommandDef(Action callback) {
            Callback = callback;
        }

        #region Collection initializer
        public IEnumerator GetEnumerator() {
            throw new NotImplementedException();
        }

        private void Add(char? shortcut, string prototype, Option option) {
            prototype = prototype.Trim();
            if (shortcut == null && string.IsNullOrEmpty(prototype)) {
                throw new OptionsDefException();
            }
            _options.Add(option);
            if (shortcut != null) {
                OptionsByShortcut.Add(shortcut.Value, option);
            }
            if (!string.IsNullOrEmpty(prototype)) {
                OptionsByPrototype.Add(prototype, option);
            }
        }

        #region Add switch option
        public void Add(char? shortcut, string prototype, string description, Action callback) {
            Add(shortcut, prototype, new SwitchOption(shortcut, prototype, description, callback));
        }

        public void Add(string prototype, string description, Action callback) {
            Add(null, prototype, description, callback);
        }

        public void Add(char shortcut, string description, Action callback) {
            Add(shortcut, null, description, callback);
        }
        #endregion

        #region Add argument option
        public void Add<T>(char? shortcut, string prototype, string description, Action<T> callback) {
            Add(shortcut, prototype, ParamOption.Create(shortcut, prototype, description, callback));
        }

        public void Add<T>(string prototype, string description, Action<T> callback) {
            Add(null, prototype, description, callback);
        }

        public void Add<T>(char shortcut, string description, Action<T> callback) {
            Add(shortcut, null, description, callback);
        }
        #endregion

        #region Add subcommand
        public void Add(string commandName, string description, CommandDef subCommandDef) {
            if (string.IsNullOrEmpty(commandName)) {
                throw new OptionsDefException();
            }
            var subCommand = new SubCommand {
                CommandName = commandName,
                Description = description,
                CommandDef = subCommandDef
            };
            _subCommands.Add(subCommand);
            SubCommandsByName.Add(commandName, subCommand);
        }
        #endregion
        #endregion
    }

    internal class SubCommand {
        public string CommandName { get; set; }
        public string Description { get; set; }
        public CommandDef CommandDef { get; set; }
    }

    public class OptionsDefException : Exception {}

    public class OptionsParseException : Exception {}

    #region Options
    internal abstract class Option {
        private char? _shortcut;
        private string _prototype;
        private string _description;

        protected Option(char? shortcut, string prototype, string description) {
            _shortcut = shortcut;
            _prototype = prototype;
            _description = description;
        }

        internal void Output(TextWriter output) {
            output.WriteLine();
        }

        internal abstract bool Process(string value); // returns whether the value is consumed
    }

    internal class SwitchOption : Option {
        private readonly Action _callback;

        public SwitchOption(char? shortcut, string prototype, string description, Action callback)
            : base(shortcut, prototype, description) {
            _callback = callback;
        }

        internal override bool Process(string value) {
            _callback();
            // value unused
            return false;
        }
    }

    internal class ParamOption : Option {
        private readonly Action<string> _callback;

        public ParamOption(char? shortcut, string prototype, string description, Action<string> callback)
            : base(shortcut, prototype, description) {
            _callback = callback;
        }

        public static ParamOption Create<T>(
                char? shortcut, string prototype, string description, Action<T> action) {
            return new ParamOption(shortcut, prototype, description, v => action(
                (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(v)));
        }

        internal override bool Process(string value) {
            if (value != null) {
                _callback(value);
            }
            // value is consumed
            return true;
        }
    } 
    #endregion

    public static class ArgParser {
        public static List<string> Parse(this CommandDef command, string[] args) {
            var parser = new Parser(args);

            return parser.Parse(command);
        }
    }

    internal class Parser {
        private readonly string[] _args;
        private int _offset;
        private string _arg;
        private string _option;
        private string _param;
        private int _next; // if _param is consumed, move to _next, otherwise, move to _offset

        public Parser(string[] args) {
            _args = args;
        }

        public List<string> Parse(CommandDef command) {
            var extra = new List<string>();
            _offset = 0;
            while (_offset < _args.Length) {
                _arg = _args[_offset];
                _offset += 1;
                if (_arg[0] != '-') { // not starts with dash, either command or arugment
                    // command must be prior to any extra argument,
                    // so look for command only if there's no extra argument yet
                    SubCommand subCommand;
                    if (extra.Count == 0 && command.SubCommandsByName.TryGetValue(_arg, out subCommand)) {
                        command = subCommand.CommandDef;
                        continue;
                    }
                    // not a registered subcommand
                    extra.Add(_arg);
                    continue;
                }

                // token starts with dash, check if it is shortcut or prototype

                if (_arg.Length == 1) {
                    // a single dash can't be intepreted, raise an error
                    throw new OptionsParseException();
                }

                ScanParam();
                Option option;
                if (_arg[1] != '-') {
                    // start with a single dash, it is shortcut(s)
                    var shortcuts = _option.Substring(1);
                    // process combined shortcuts except last one, they must be switch options
                    foreach (var shortcut in shortcuts.Substring(0, shortcuts.Length - 1)) {
                        if (!command.OptionsByShortcut.TryGetValue(shortcut, out option)) {
                            // unknown shortcut
                            throw new OptionsParseException();
                        }

                        if (option.Process(_param)) {
                            // they should not consume the param, param is for last shortcut
                            throw new OptionsParseException();
                        }
                    }
                    // process last shortcut
                    if (!command.OptionsByShortcut.TryGetValue(shortcuts[shortcuts.Length - 1], out option)) {
                        // unknown shortcut
                        throw new OptionsParseException();
                    }
                    if (option.Process(_param)) {
                        if (_param == null) {
                            // the option need a param, but not provided
                            throw new OptionsParseException();
                        }
                        _offset = _next;
                    } else {
                        if (_param != null && _offset == _next) {
                            // param is provide in arg, but the option does not consume it
                            throw new OptionsParseException();
                        }
                    }
                    continue;
                }

                // starts with double dashes
                if (_arg.Length == 2) {
                    // standalone double dashes means no more option or command
                    // then the rest args are extra arguments
                    for(; _offset < _args.Length; _offset++)
                        extra.Add(_args[_offset]);
                    continue;
                }

                // prototype
                var prototype = _arg.Substring(2);
                if (!command.OptionsByPrototype.TryGetValue(prototype, out option)) {
                    throw new OptionsParseException();
                }
                if (option.Process(_param)) {
                    if (_param == null) {
                        // the option need a param, but not provided
                        throw new OptionsParseException();
                    }
                    _offset = _next;
                } else {
                    if (_param != null && _offset == _next) {
                        // param is provide in arg, but the option does not consume it
                        throw new OptionsParseException();
                    }
                }
            }
            // no more args, parse finished on deepest command
            command.Callback();
            return extra;
        }

        // TODO: support args like --arg="foo bar"
        private void ScanParam() {
            var tokenParts = _arg.Split(new []{'='}, 2);
            _option = tokenParts[0];
            if (tokenParts.Length > 1) {
                _param = tokenParts[1];
                _next = _offset;
                return;
            }
            if (_offset < _args.Length) {
                _param = _args[_offset];
                _next = _offset + 1;
            } else {
                _param = null;
                _next = _offset;
            }
        }
    }
}
