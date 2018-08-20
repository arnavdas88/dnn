// -----------------------------------------------------------------------
// <copyright file="CommandLineOption.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Represents the option for the <see cref="CommandLineCommand"/>.
    /// </summary>
    public sealed class CommandLineOption : CommandLineSwitch
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineOption"/> class.
        /// </summary>
        /// <param name="name">The option name.</param>
        /// <param name="valueName">The option value name.</param>
        /// <param name="description">The option description.</param>
        /// <param name="types">The option types.</param>
        public CommandLineOption(
            string name,
            string valueName,
            string description,
            CommandLineOptionTypes types)
            : base(name, description)
        {
            this.ValueName = valueName;
            this.Types = types;
        }

        /// <summary>
        /// Gets the option value name.
        /// </summary>
        /// <value>
        /// The option value name.
        /// </value>
        public string ValueName { get; }

        /// <summary>
        /// Gets the option types.
        /// </summary>
        /// <value>
        /// The <see cref="CommandLineOptionTypes"/> enumeration.
        /// </value>
        public CommandLineOptionTypes Types { get; }

        /// <summary>
        /// Gets the option value.
        /// </summary>
        /// <value>
        /// The option value.
        /// </value>
        public string Value { get; private set; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (this.Types.HasFlag(CommandLineOptionTypes.Required))
            {
                return string.Format(CultureInfo.InvariantCulture, "--{0}|-{1}=<{2}>", this.Name, this.Name[0], this.ValueName);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "[--{0}|-{1}=<{2}>]", this.Name, this.Name[0], this.ValueName);
            }
        }

        /// <inheritdoc />
        internal override bool Parse(string arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException("arg");
            }

            if (this.Exists)
            {
                return false;
            }

            string key1 = "--" + this.Name + "=";
            string key2 = "-" + this.Name[0] + "=";
            if (arg.StartsWith(key1, StringComparison.Ordinal))
            {
                arg = arg.Substring(key1.Length);
            }
            else if (arg.StartsWith(key2, StringComparison.Ordinal))
            {
                arg = arg.Substring(key2.Length);
            }
            else
            {
                return false;
            }

            arg = arg.Unqualify('\"');

            if (this.Types.HasFlag(CommandLineOptionTypes.FileMustExist))
            {
                if (!File.Exists(arg))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The file <{0}> does not exist.", arg));
                }
            }

            if (this.Types.HasFlag(CommandLineOptionTypes.PathMustExist))
            {
                string directoryName = Path.HasExtension(arg) ? Path.GetDirectoryName(arg) : arg;
                if (!Directory.Exists(directoryName))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The path <{0}> does not exist.", directoryName));
                }
            }

            if (this.Types.HasFlag(CommandLineOptionTypes.Integer))
            {
                if (!int.TryParse(arg, out int value))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The value <{0}> must represent a number.", arg));
                }
            }

            if (this.Types.HasFlag(CommandLineOptionTypes.DateTime))
            {
                if (!DateTime.TryParse(arg, out DateTime value))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The value <{0}> must represent a date.", arg));
                }
            }

            this.Exists = true;
            this.Value = arg;
            return true;
        }
    }
}
