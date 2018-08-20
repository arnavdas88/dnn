// -----------------------------------------------------------------------
// <copyright file="CommandLineArgument.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Represents the argument for the <see cref="CommandLineCommand"/>.
    /// </summary>
    public sealed class CommandLineArgument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineArgument"/> class.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="description">The argument description.</param>
        /// <param name="types">The argument types.</param>
        public CommandLineArgument(string name, string description, CommandLineArgumentTypes types)
        {
            this.Name = name;
            this.Description = description;
            this.Types = types;
        }

        /// <summary>
        /// Gets the argument name.
        /// </summary>
        /// <value>
        /// The argument name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the argument description.
        /// </summary>
        /// <value>
        /// The argument description.
        /// </value>
        public string Description { get; }

        /// <summary>
        /// Gets the argument options.
        /// </summary>
        /// <value>
        /// The argument options.
        /// </value>
        public CommandLineArgumentTypes Types { get; }

        /// <summary>
        /// Gets the argument value.
        /// </summary>
        /// <value>
        /// The argument value.
        /// </value>
        public string Value { get; private set; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (this.Types.HasFlag(CommandLineArgumentTypes.Optional))
            {
                return string.Format(CultureInfo.InvariantCulture, "[<{0}>]", this.Name);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "<{0}>", this.Name);
            }
        }

        /// <summary>
        /// Parses the command-line argument.
        /// </summary>
        /// <param name="arg">The command line argument.</param>
        /// <returns><b>true</b> if the argument was parsed successfully; otherwise, <b>false</b>.</returns>
        public bool Parse(string arg)
        {
            if (this.Types.HasFlag(CommandLineArgumentTypes.FileMustExist))
            {
                if (!File.Exists(arg))
                {
                    if (!this.Types.HasFlag(CommandLineArgumentTypes.PathMustExist))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The file <{0}> does not exist.", arg));
                    }
                }
            }

            if (this.Types.HasFlag(CommandLineArgumentTypes.PathMustExist))
            {
                string directoryName = Path.HasExtension(arg) ? Path.GetDirectoryName(arg) : arg;
                if (!Directory.Exists(directoryName))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The path <{0}> does not exist.", directoryName));
                }
            }

            if (this.Types.HasFlag(CommandLineArgumentTypes.Integer))
            {
                if (!int.TryParse(arg, out int value))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The value <{0}> must represent a number.", arg));
                }
            }

            if (this.Types.HasFlag(CommandLineArgumentTypes.DateTime))
            {
                if (!DateTime.TryParse(arg, out DateTime value))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The value <{0}> must represent a date.", arg));
                }
            }

            this.Value = arg;
            return true;
        }
    }
}
