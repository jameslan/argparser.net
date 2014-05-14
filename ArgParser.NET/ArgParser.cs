using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace ArgParser.NET {
    public class OptionSet : IEnumerable {
        private List<SubCommand> _subCommands = new List<SubCommand>();
        private Dictionary<string, SubCommand> _subCommandsByName = new Dictionary<string, SubCommand>();
        private Dictionary<char, Option> _optionsByShortcut = new Dictionary<char, Option>();
        private Dictionary<string, Option> _optionsByPrototype = new Dictionary<string, Option>();
        private List<Option> _options = new List<Option>();

        public Action Action { get; private set; }

        public OptionSet(Action action) {
            Action = action;
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
                _optionsByShortcut.Add(shortcut.Value, option);
            }
            if (!string.IsNullOrEmpty(prototype)) {
                _optionsByPrototype.Add(prototype, option);
            }
        }

        #region Add switch option
        public void Add(char? shortcut, string prototype, string description, Action action) {
            Add(shortcut, prototype, new SwitchOption(shortcut, prototype, description, action));
        }

        public void Add(string prototype, string description, Action action) {
            Add(null, prototype, description, action);
        }

        public void Add(char shortcut, string description, Action action) {
            Add(shortcut, null, description, action);
        }
        #endregion

        #region Add argument option
        public void Add<T>(char? shortcut, string prototype, string description, Action<T> action) {
            Add(shortcut, prototype, ParamOption.Create(shortcut, prototype, description, action));
        }

        public void Add<T>(string prototype, string description, Action<T> action) {
            Add(null, prototype, description, action);
        }

        public void Add<T>(char shortcut, string description, Action<T> action) {
            Add(shortcut, null, description, action);
        }
        #endregion

        #region Add subcommand
        public void Add(string command, string description, OptionSet subOptionSet) {
            if (string.IsNullOrEmpty(command)) {
                throw new OptionsDefException();
            }
            var subCommand = new SubCommand {
                Command = command,
                Description = description,
                OptionSet = subOptionSet
            };
            _subCommands.Add(subCommand);
            _subCommandsByName.Add(command, subCommand);
        }
        #endregion
        #endregion

        public List<string> Parse(string[] args) {
            var extra = new List<string>();
            var i = 0;
            while (i < args.Length) {
                var token = args[i];
                i += 1;
                if (token[0] != '-') {
                    if (_subCommandsByName.ContainsKey(token)) {
                        
                    } else {
                        // not a subcommand
                        extra.Add(token);
                    }
                    continue;
                }
                if (token.Length == 1) {
                    // a single dash, raise an error
                }
                if (token[1] != '-') {
                    // start with a single dash, it must be shortcut(s)
                    string value;
                    var tokenParts = token.Split(new []{'='}, 2);
                    if (tokenParts.Length > 1) {
                        value = tokenParts[1];
                    } else {
                        if (i < args.Length) {
                            value = args[i];
                        }
                    }
                    foreach (var shortcut in tokenParts[0].Substring(2)) {
                        if (!_optionsByShortcut.ContainsKey(shortcut)) {
                            // unknown shortcut
                        }
                        _optionsByShortcut[shortcut].Process(value);
                    }
                }
            }
            return extra;
        }
    }

    internal class SubCommand {
        public string Command { get; set; }
        public string Description { get; set; }
        public OptionSet OptionSet { get; set; }
    }

    public class OptionsDefException : Exception {}

    public class OptionsParseException : Exception {}

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

        internal abstract void Process(string value);
    }

    internal class SwitchOption : Option {
        private readonly Action _action;

        public SwitchOption(char? shortcut, string prototype, string description, Action action)
            : base(shortcut, prototype, description) {
            _action = action;
        }

        internal override void Process(string value) {
            // value unused
            _action();
        }
    }

    internal class ParamOption : Option {
        private readonly Action<string> _action;

        public ParamOption(char? shortcut, string prototype, string description, Action<string> action)
            : base(shortcut, prototype, description) {
            _action = action;
        }

        public static ParamOption Create<T>(
                char? shortcut, string prototype, string description, Action<T> action) {
            return new ParamOption(shortcut, prototype, description, v => action(
                (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(v)));
        }

        internal override void Process(string value) {
            _action(value);
        }
    } 
}
