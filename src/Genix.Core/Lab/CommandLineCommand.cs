// -----------------------------------------------------------------------
// <copyright file="CommandLineCommand.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents the command for the <see cref="CommandLineParser"/>.
    /// </summary>
    public class CommandLineCommand
    {
        /// <summary>
        /// The help switch.
        /// </summary>
        private static CommandLineSwitch switchHelp = new CommandLineSwitch("help", string.Empty);

        /// <summary>
        /// The command sub-commands.
        /// </summary>
        private readonly List<CommandLineCommand> commands = new List<CommandLineCommand>();

        /// <summary>
        /// The command arguments.
        /// </summary>
        private readonly List<CommandLineArgument> arguments = new List<CommandLineArgument>();

        /// <summary>
        /// The command switches.
        /// </summary>
        private readonly List<CommandLineSwitch> options = new List<CommandLineSwitch>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineCommand"/> class.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="description">The command description.</param>
        public CommandLineCommand(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Gets the command name.
        /// </summary>
        /// <value>
        /// The command name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the command description.
        /// </summary>
        /// <value>
        /// The command description.
        /// </value>
        public string Description { get; }

        /// <summary>
        /// Adds a new command.
        /// </summary>
        /// <param name="command">The command to add.</param>
        public void AddCommand(CommandLineCommand command) => this.commands.Add(command);

        /// <summary>
        /// Adds a new command.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="description">The command description.</param>
        /// <returns>
        /// The <see cref="CommandLineCommand"/> object this method creates.
        /// </returns>
        public CommandLineCommand AddCommand(string name, string description)
        {
            CommandLineCommand command = new CommandLineCommand(name, description);
            this.commands.Add(command);
            return command;
        }

        /// <summary>
        /// Gets the command by name.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <returns>
        /// The <see cref="CommandLineCommand"/> object.
        /// </returns>
        public CommandLineCommand FindCommand(string name) => this.commands.FirstOrDefault(x => x.Name == name);

        /// <summary>
        /// Adds a new argument.
        /// </summary>
        /// <param name="argument">The argument to add.</param>
        public void AddArgument(CommandLineArgument argument) => this.arguments.Add(argument);

        /// <summary>
        /// Adds a new argument.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="types">The argument types.</param>
        /// <returns>
        /// The <see cref="CommandLineArgument"/> object this method creates.
        /// </returns>
        public CommandLineArgument AddArgument(string name, string description, CommandLineArgumentTypes types)
        {
            CommandLineArgument argument = new CommandLineArgument(name, description, types);
            this.arguments.Add(argument);
            return argument;
        }

        /// <summary>
        /// Gets the argument by name.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <returns>
        /// The <see cref="CommandLineArgument"/> object.
        /// </returns>
        public CommandLineArgument FindArgument(string name) => this.arguments.FirstOrDefault(x => x.Name == name);

        /// <summary>
        /// Adds a new switch.
        /// </summary>
        /// <param name="switch">The switch to add.</param>
        public void AddSwitch(CommandLineSwitch @switch) => this.options.Add(@switch);

        /// <summary>
        /// Adds a new switch.
        /// </summary>
        /// <param name="name">The switch name.</param>
        /// <param name="description">The switch description.</param>
        /// <returns>
        /// The <see cref="CommandLineSwitch"/> object this method creates.
        /// </returns>
        public CommandLineSwitch AddSwitch(string name, string description)
        {
            CommandLineSwitch @switch = new CommandLineSwitch(name, description);
            this.options.Add(@switch);
            return @switch;
        }

        /// <summary>
        /// Gets the switch by name.
        /// </summary>
        /// <param name="name">The switch name.</param>
        /// <returns>
        /// The <see cref="CommandLineCommand"/> object.
        /// </returns>
        public CommandLineSwitch FindSwitch(string name) => this.options.FirstOrDefault(x => x.Name == name);

        /// <summary>
        /// Adds a new option.
        /// </summary>
        /// <param name="option">The option to add.</param>
        public void AddOption(CommandLineOption option) => this.options.Add(option);

        /// <summary>
        /// Adds a new option.
        /// </summary>
        /// <param name="name">The option name.</param>
        /// <param name="valueName">The option value name.</param>
        /// <param name="description">The option description.</param>
        /// <param name="types">The option types.</param>
        /// <returns>
        /// The <see cref="CommandLineOption"/> object this method creates.
        /// </returns>
        public CommandLineOption AddOption(string name, string valueName, string description, CommandLineOptionTypes types)
        {
            CommandLineOption option = new CommandLineOption(name, valueName, description, types);
            this.options.Add(option);
            return option;
        }

        /// <summary>
        /// Gets the option by name.
        /// </summary>
        /// <param name="name">The option name.</param>
        /// <returns>
        /// The <see cref="CommandLineOption"/> object.
        /// </returns>
        public CommandLineOption FindOption(string name) => this.options.OfType<CommandLineOption>().FirstOrDefault(x => x.Name == name);

        /// <summary>
        /// Parses the command-line arguments.
        /// </summary>
        /// <param name="prefix">The command prefix that includes application name and parent commands.</param>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>
        /// The parsed <see cref="CommandLineCommand"/>.
        /// </returns>
        internal CommandLineCommand Parse(string prefix, string[] args)
        {
            if (args.Length == 1 && CommandLineCommand.switchHelp.Parse(args[0]))
            {
                goto err;
            }

            if (this.commands.Count > 0)
            {
                if (args.Length == 0)
                {
                    goto err;
                }

                CommandLineCommand command = this.commands.Find(x => x.Name == args[0]);
                if (command == null)
                {
                    goto err;
                }

                return command.Parse(string.Join(" ", prefix, this.Name), args.Skip(1).ToArray());
            }
            else
            {
                // parse switches and parameters
                foreach (CommandLineSwitch @switch in this.options)
                {
                    bool found = false;
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (@switch.Parse(args[i]))
                        {
                            args.RemoveAt(i);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        if (@switch is CommandLineOption missingParameter &&
                            missingParameter.Types.HasFlag(CommandLineOptionTypes.Required))
                        {
                            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The parameter <{0}> is required.", missingParameter.Name));
                        }
                    }
                }

                // parse arguments
                if (args.Length > this.arguments.Count)
                {
                    goto err;
                }

                for (int i = 0; i < args.Length && i < this.arguments.Count; i++)
                {
                    if (!this.arguments[i].Parse(args[i]))
                    {
                        goto err;
                    }
                }

                // validate required arguments
                CommandLineArgument missingArgument = this.arguments
                    .FirstOrDefault(x => string.IsNullOrEmpty(x.Value) && !x.Types.HasFlag(CommandLineArgumentTypes.Optional));
                if (missingArgument != null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The argument <{0}> is required.", missingArgument.Name));
                }

                return this;
            }

err:
            this.PrintUsage(prefix);
            return null;
        }

        internal IEnumerable<string> BuildUsage(string prefix, bool printDescription, bool fullInfo)
        {
            if (printDescription && !string.IsNullOrEmpty(this.Description))
            {
                yield return this.Description;
                yield return string.Empty;
            }

            if (this.commands.Count > 0)
            {
                prefix = string.Join(" ", prefix, this.Name);
                foreach (CommandLineCommand command in this.commands)
                {
                    foreach (string s in command.BuildUsage(prefix, false, false))
                    {
                        yield return s;
                    }
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1}", prefix, this.Name);

                List<(string name, string description)> args = new List<(string, string)>();
                args.AddRange(this.arguments.Select(x => (x.ToString(), x.Description)));
                args.AddRange(this.options.Select(x => (x.ToString(), x.Description)));

                if (args.Count > 0)
                {
                    sb.Append(" ");
                    sb.Append(string.Join(" ", args.Select(x => x.name)));
                }

                yield return sb.ToString();

                if (fullInfo && args.Count > 0)
                {
                    yield return string.Empty;

                    int maxLength = Math.Max((args.Max(x => x.name.Length) + 7) / 8 * 8, 16);
                    foreach ((string name, string description) in args)
                    {
                        yield return name + new string(' ', maxLength - name.Length) + description;
                    }
                }
            }
        }

        private void PrintUsage(string prefix)
        {
            foreach (string s in this.BuildUsage(prefix, true, true))
            {
                Console.WriteLine(s);
            }

            Console.WriteLine();
        }
    }
}