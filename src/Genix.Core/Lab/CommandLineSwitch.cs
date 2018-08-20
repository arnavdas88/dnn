// -----------------------------------------------------------------------
// <copyright file="CommandLineSwitch.cs" company="Noname, Inc.">
// Copyright (c) 2018, Alexander Volgunin. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Genix.Lab
{
    using System.Globalization;

    /// <summary>
    /// Represents the switch for the <see cref="CommandLineParser"/>.
    /// </summary>
    public class CommandLineSwitch
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineSwitch"/> class.
        /// </summary>
        /// <param name="name">The switch name.</param>
        /// <param name="description">The command description.</param>
        public CommandLineSwitch(string name, string description)
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
        /// Gets or sets a value indicating whether this switch exists in the command line.
        /// </summary>
        /// <value>
        /// <b>true</b> if the switch exists; otherwise, <b>false</b>.
        /// </value>
        public bool Exists { get; protected set; }

        /// <inheritdoc />
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "[--{0}|-{1}]", this.Name, this.Name[0]);

        /// <summary>
        /// Validates that the command line argument matches the switch.
        /// </summary>
        /// <param name="arg">The command line argument.</param>
        /// <returns><b>true</b> if command line matches the switch; otherwise, <b>false</b>.</returns>
        internal virtual bool Parse(string arg)
        {
            if (this.Exists)
            {
                return false;
            }

            this.Exists = arg == "--" + this.Name || arg == "-" + this.Name[0];
            return this.Exists;
        }
    }
}
