// -----------------------------------------------------------------------
// <copyright file="CommandLineParser.cs" company="Noname, Inc.">
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
    /// Parses the command-line arguments in command mode.
    /// </summary>
    public sealed class CommandLineParser
    {
        /// <summary>
        /// The application name.
        /// </summary>
        private readonly string applicationName;

        /// <summary>
        /// The list of command the parser can parse.
        /// </summary>
        private readonly List<CommandLineCommand> commands = new List<CommandLineCommand>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParser"/> class.
        /// </summary>
        /// <param name="applicationName">The application name.</param>
        public CommandLineParser(string applicationName)
        {
            this.applicationName = applicationName;

            CommandLineCommand helpCommand = new CommandLineCommand("help", "display usage info");
            helpCommand.AddSwitch("long", "display full usage info");
            this.commands.Add(helpCommand);
        }

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
        /// Parses the command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>
        /// The <see cref="CommandLineCommand"/> object that represents the command that was parsed.
        /// </returns>
        public CommandLineCommand Parse(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                this.PrintUsage(false);
                return null;
            }

            CommandLineCommand command = this.commands.Find(x => x.Name == args[0]);
            if (command == null)
            {
                this.PrintUsage(false);
                return null;
            }

            command = command.Parse(this.applicationName, args.Skip(1).ToArray());
            if (command?.Name == "help")
            {
                this.PrintUsage(command.FindSwitch("long").Exists);
                return null;
            }

            return command;
        }

        private static IEnumerable<string> Join(string separator, IEnumerable<string> values, int maxlength)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string s in values)
            {
                if (sb.Length > 0)
                {
                    if (sb.Length + separator.Length + s.Length > maxlength)
                    {
                        sb.Append(separator.TrimEnd());

                        yield return sb.ToString();
                        sb.Clear();
                    }
                }

                if (sb.Length > 0)
                {
                    sb.Append(separator);
                }

                sb.Append(s);
            }

            if (sb.Length > 0)
            {
                yield return sb.ToString();
            }
        }

        private void PrintUsage(bool fullInfo)
        {
            foreach (string s in this.BuildUsage(fullInfo))
            {
                Console.WriteLine(s);
            }

            Console.WriteLine();
        }

        private IEnumerable<string> BuildUsage(bool fullInfo)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "Usage: {0} <command>", this.applicationName);
            yield return string.Empty;
            yield return "where <command> is one of:";
            if (fullInfo)
            {
                int maxCommandNameLength = Math.Max(this.commands.Max(x => x.Name.Length) + 1, 8);

                foreach (CommandLineCommand command in this.commands.OrderBy(x => x.Name))
                {
                    yield return string.Empty;

                    string[] commandUsage = command.BuildUsage(this.applicationName, true, false).ToArray();
                    if (commandUsage.Length == 0)
                    {
                        yield return new string(' ', 4) + command.Name;
                    }
                    else
                    {
                        for (int i = 0; i < commandUsage.Length; i++)
                        {
                            if (i == 0)
                            {
                                yield return new string(' ', 4) +
                                    command.Name +
                                    new string(' ', maxCommandNameLength - command.Name.Length) +
                                    commandUsage[i];
                            }
                            else
                            {
                                yield return new string(' ', 4 + maxCommandNameLength) + commandUsage[i];
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (string s in CommandLineParser.Join(", ", this.commands.Select(x => x.Name).OrderBy(x => x), 80))
                {
                    yield return new string(' ', 4) + s;
                }
            }

            yield return string.Empty;

            List<(string name, string description)> additionalHelp = new List<(string, string)>
            {
                (string.Format(CultureInfo.InvariantCulture, "{0} help", this.applicationName), "display quick usage info"),
                (string.Format(CultureInfo.InvariantCulture, "{0} help -l", this.applicationName), "display full usage info"),
                (string.Format(CultureInfo.InvariantCulture, "{0} <command> -h", this.applicationName), "display help on <command>"),
            };

            int maxLength = Math.Max((additionalHelp.Max(x => x.name.Length) + 7) / 8 * 8, 16);
            foreach ((string name, string description) in additionalHelp)
            {
                yield return name + new string(' ', maxLength - name.Length) + description;
            }
        }
    }
}
