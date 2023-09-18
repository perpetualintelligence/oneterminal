﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Exceptions;
using System.Collections.Generic;

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// An immutable <c>pi-cli</c> command. A command is a specific action or a set of actions that a user or an
    /// application requests the underlying system to perform. It can be a simple action such as invoking a system
    /// method or an OS command or representing a complex operation that calls a set of protected APIs over the internal
    /// or external network. A command can virtually do anything in the context of your application or service.
    /// </summary>
    /// <seealso cref="CommandDescriptor"/>
    /// <seealso cref="Option"/>
    /// <seealso cref="Commands.Options"/>
    public sealed class Command
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="commandDescriptor">The command descriptor.</param>
        /// <param name="options">The command options.</param>
        /// <param name="arguments">The command arguments.</param>
        public Command(CommandDescriptor commandDescriptor, Options? options = null, Arguments? arguments = null)
        {
            Descriptor = commandDescriptor ?? throw new System.ArgumentNullException(nameof(commandDescriptor));
            Options = options;
            Arguments = arguments;
        }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor Descriptor { get; }

        /// <summary>
        /// The command options.
        /// </summary>
        public Options? Options { get; set; }

        /// <summary>
        /// The command arguments.
        /// </summary>
        public Arguments? Arguments { get; set; }

        /// <summary>
        /// The command custom properties.
        /// </summary>
        public Dictionary<string, object>? CustomProperties => Descriptor.CustomProperties;

        /// <summary>
        /// The command description.
        /// </summary>
        public string? Description => Descriptor.Description;

        /// <summary>
        /// The command id unique.
        /// </summary>
        public string Id => Descriptor.Id;

        /// <summary>
        /// The command name.
        /// </summary>
        public string Name => Descriptor.Name;

        /// <summary>
        /// Attempts to get the option for the specified identifier.
        /// </summary>
        /// <param name="idOrAlias">The option identifier or its alias.</param>
        /// <param name="option">The option if found.</param>
        /// <returns><c>true</c> if the option is found.</returns>
        public bool TryGetOption(string idOrAlias, out Option? option)
        {
            option = null;

            if (Options == null)
            {
                return false;
            }

            try
            {
                option = Options[idOrAlias];
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the option value for the specified identifier.
        /// </summary>
        /// <param name="idOrAlias">The option id or its alias.</param>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <returns>The option value.</returns>
        /// <exception cref="ErrorException">If the option is not supported.</exception>
        public TValue GetOptionValue<TValue>(string idOrAlias)
        {
            if (Options == null)
            {
                throw new ErrorException(TerminalErrors.UnsupportedOption, "The option is not supported. option={0}", idOrAlias);
            }

            return Options.GetOptionValue<TValue>(idOrAlias);
        }

        /// <summary>
        /// Gets the argument value for the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <returns>The option value.</returns>
        /// <exception cref="ErrorException">If the argument is not supported.</exception>
        public TValue GetArgumentValue<TValue>(int index)
        {
            if (Arguments == null)
            {
                throw new ErrorException(TerminalErrors.UnsupportedOption, "The argument is not supported at index. index={0}", index);
            }

            return (TValue)Arguments[index].Value;
        }

        /// <summary>
        /// Gets the argument value for the specified identifier.
        /// </summary>
        /// <param name="id">The argument id.</param>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <returns>The option value.</returns>
        /// <exception cref="ErrorException">If the argument is not supported.</exception>
        public TValue GetArgumentValue<TValue>(string id)
        {
            if (Arguments == null)
            {
                throw new ErrorException(TerminalErrors.UnsupportedOption, "The argument is not supported. argument={0}", id);
            }

            return (TValue)Arguments[id].Value;
        }
    }
}