using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace ArgParser.NET {
    public class Options : IEnumerable {
        public IEnumerator GetEnumerator() {
            throw new NotImplementedException();
        }

        internal List<Options> subCommands = new List<Options>();
        internal Dictionary<char, Option> shortcuts = new Dictionary<char, Option>();
        internal Dictionary<string, Option> prototypes = new Dictionary<string, Option>();
        internal List<Option> options = new List<Option>();

        private void Add(char? shortcut, string prototype, Option option) {
            options.Add(option);
            if (shortcut != null) {
                shortcuts.Add(shortcut.Value, option);
            }
            if (!string.IsNullOrEmpty(prototype)) {
                prototypes.Add(prototype, option);
            }
        }

        public void Add(char? shortcut, string prototype, string description, Action action) {
            Add(shortcut, prototype, new SwitchOption(shortcut, prototype, description, action));
        }

        public void Add(string prototype, string description, Action action) {
            Add(null, prototype, description, action);
        }

        public void Add(char shortcut, string description, Action action) {
            Add(shortcut, null, description, action);
        }

        public void Add<T>(char? shortcut, string prototype, string description, Action<T> action) {
            Add(shortcut, prototype, ParamOption.Create(shortcut, prototype, description, action));
        }

        public void Add<T>(string prototype, string description, Action<T> action) {
            Add(null, prototype, description, action);
        }

        public void Add<T>(char shortcut, string description, Action<T> action) {
            Add(shortcut, null, description, action);
        }

        public void Add(string subcommand, string description, Action action, Options subOption) {
        }
        
    }

    internal class Option {
        private char? _shortcut;
        private string _prototype;
        private string _description;

        protected Option(char? shortcut, string prototype, string description) {
            _shortcut = shortcut;
            _prototype = prototype;
            _description = description;
        }
    }

    internal class SwitchOption : Option {
        private Action _action;

        public SwitchOption(char? shortcut, string prototype, string description, Action action)
            : base(shortcut, prototype, description) {
            _action = action;
        }
    }

    internal class ParamOption : Option {
        private Action<string> _action;

        public ParamOption(char? shortcut, string prototype, string description, Action<string> action)
            : base(shortcut, prototype, description) {
            _action = action;
        }

        public static ParamOption Create<T>(
                char? shortcut, string prototype, string description, Action<T> action) {
            return new ParamOption(shortcut, prototype, description, v => action(
                (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(v)));
        }
    } 

    public static class ArgParser {
        public static List<string> Parse(this Options options, string[] args) {
            var extra = new List<string>();
            return extra;
        }

    }
}
