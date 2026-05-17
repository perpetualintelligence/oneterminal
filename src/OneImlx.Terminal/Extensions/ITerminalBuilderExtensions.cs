//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Dynamics;
using OneImlx.Terminal.Events;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Shared.Declarative;
using OneImlx.Terminal.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// The <see cref="ITerminalBuilder"/> extension methods.
    /// </summary>
    public static class ITerminalBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="ITerminalBytesParser"/> to the service collection.
        /// </summary>
        /// <typeparam name="TBytesParser"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ITerminalBuilder AddBytesParser<TBytesParser>(this ITerminalBuilder builder) where TBytesParser : class, ITerminalBytesParser
        {
            builder.Services.AddSingleton<ITerminalBytesParser, TBytesParser>();
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="IDataTypeMapper{TValue}"/> and <see cref="IArgumentChecker"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TMapper">The argument mapper type.</typeparam>
        /// <typeparam name="TChecker">The argument checker type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddArgumentChecker<TMapper, TChecker>(this ITerminalBuilder builder) where TMapper : class, IDataTypeMapper<Argument> where TChecker : class, IArgumentChecker
        {
            builder.Services.AddTransient<IDataTypeMapper<Argument>, TMapper>();
            builder.Services.AddTransient<IArgumentChecker, TChecker>();
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ICommandParser"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TCommand">The command parser type.</typeparam>
        /// <typeparam name="TRequest">The terminal request parser type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddCommandParser<TCommand, TRequest>(this ITerminalBuilder builder) where TCommand : class, ICommandParser where TRequest : class, ITerminalRequestParser
        {
            // Add command parser
            builder.Services.AddTransient<ICommandParser, TCommand>();

            // Add option parser
            builder.Services.AddTransient<ITerminalRequestParser, TRequest>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ICommandRouter"/> and <see cref="ICommandHandler"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddCommandRouter<TRouter, THandler, TResolver>(this ITerminalBuilder builder) where TRouter : class, ICommandRouter where THandler : class, ICommandHandler where TResolver : class, ICommandResolver
        {
            // Add command router
            builder.Services.AddTransient<ICommandRouter, TRouter>();

            // Add command handler
            builder.Services.AddTransient<ICommandHandler, THandler>();

            // Add command runtime
            builder.Services.AddTransient<ICommandResolver, TResolver>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITerminalCommandStore"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TStore">The command descriptor store type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddCommandStore<TStore>(this ITerminalBuilder builder)
            where TStore : class, ITerminalCommandStore
        {
            builder.Services.AddSingleton<ITerminalCommandStore, TStore>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="TerminalOptions"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddConfigurationOptions(this ITerminalBuilder builder)
        {
            // Add options.
            builder.Services.AddOptions();
            builder.Services.AddSingleton(static resolver => resolver.GetRequiredService<IOptions<TerminalOptions>>().Value);

            // Add options checker
            builder.Services.AddSingleton<IConfigurationOptionsChecker, ConfigurationOptionsChecker>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITerminalConsole"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddConsole<TConsole>(this ITerminalBuilder builder) where TConsole : class, ITerminalConsole
        {
            // Add terminal routing service.
            builder.Services.AddSingleton<ITerminalConsole, TConsole>();

            return builder;
        }

        /// <summary>
        /// Adds all the <see cref="IDeclarativeRunner"/> implementations to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assembly">The assembly to inspect.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="AddDeclarativeAssembly(ITerminalBuilder, Assembly)"/> reads the target assembly and inspects
        /// all the declarative targets using reflection. Reflection may have a performance bottleneck. For more
        /// optimized and direct declarative target inspection, use <see cref="AddDeclarativeRunner{TDeclarativeRunner}(ITerminalBuilder)"/>.
        /// </remarks>
        public static ITerminalBuilder AddDeclarativeAssembly(this ITerminalBuilder builder, Assembly assembly)
        {
            IEnumerable<Type> declarativeTypes = assembly.GetTypes().Where(static e => typeof(IDeclarativeRunner).IsAssignableFrom(e) && !e.IsAbstract && !e.IsInterface);

            foreach (Type type in declarativeTypes)
            {
                AddDeclarativeRunnerInner(builder, type);
            }

            return builder;
        }

        /// <summary>
        /// Adds all the <see cref="IDeclarativeRunner"/> implementations to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TType">The type whose assembly to inspect and read all the declarative targets.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="AddDeclarativeAssembly{TType}(ITerminalBuilder)"/> reads the target assembly and inspects all
        /// the declarative targets using reflection. Reflection may have a performance bottleneck. For more optimized
        /// and direct declarative target inspection, use <see cref="AddDeclarativeRunner{TDeclarativeRunner}(ITerminalBuilder)"/>.
        /// </remarks>
        public static ITerminalBuilder AddDeclarativeAssembly<TType>(this ITerminalBuilder builder)
        {
            return AddDeclarativeAssembly(builder, typeof(TType).Assembly);
        }

        /// <summary>
        /// Adds a <see cref="IDeclarativeRunner"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>
        /// The <see cref="AddDeclarativeRunner{TDeclarativeRunner}(ITerminalBuilder)"/> inspects the declarative target
        /// type using reflection.
        /// </returns>
        public static ITerminalBuilder AddDeclarativeRunner<TDeclarativeRunner>(this ITerminalBuilder builder) where TDeclarativeRunner : IDeclarativeRunner
        {
            return AddDeclarativeRunnerInner(builder, typeof(TDeclarativeRunner));
        }

        /// <summary>
        /// Adds dynamics services to the terminal builder for loading commands from an external source. This method
        /// registers the specified command source, command source checker, and command source assembly loader as
        /// singletons in the service collection.
        /// </summary>
        /// <typeparam name="TContext">
        /// The type of the context used in the command source, checker, and loader. Must be a class.
        /// </typeparam>
        /// <typeparam name="TSource">The type of the terminal command source. Must be a class implementing <see cref="ITerminalCommandSource{TContext}"/>.</typeparam>
        /// <typeparam name="TChecker">
        /// The type of the terminal command source checker. Must be a class implementing <see cref="ITerminalCommandSourceChecker{TContext}"/>.
        /// </typeparam>
        /// <typeparam name="TLoader">
        /// The type of the terminal command source assembly loader. Must be a class implementing <see cref="ITerminalCommandSourceAssemblyLoader{TContext}"/>.
        /// </typeparam>
        /// <param name="builder">The terminal builder to which the integration services are added.</param>
        /// <returns>The <see cref="ITerminalBuilder"/> with the added integration services, enabling method chaining.</returns>
        public static ITerminalBuilder AddDynamics<TContext, TSource, TChecker, TLoader>(this ITerminalBuilder builder)
            where TContext : class
            where TSource : class, ITerminalCommandSource<TContext>
            where TChecker : class, ITerminalCommandSourceChecker<TContext>
            where TLoader : class, ITerminalCommandSourceAssemblyLoader<TContext>
        {
            builder.Services.AddSingleton<ITerminalCommandSource<TContext>, TSource>();
            builder.Services.AddSingleton<ITerminalCommandSourceChecker<TContext>, TChecker>();
            builder.Services.AddSingleton<ITerminalCommandSourceAssemblyLoader<TContext>, TLoader>();
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITerminalEventHandler"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddEventHandler<TEventHandler>(this ITerminalBuilder builder) where TEventHandler : class, ITerminalEventHandler
        {
            builder.Services.AddSingleton<ITerminalEventHandler, TEventHandler>();
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITerminalExceptionHandler"/> to the service collection.
        /// </summary>
        /// <typeparam name="THandler">The <see cref="ITerminalExceptionHandler"/> type.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddExceptionHandler<THandler>(this ITerminalBuilder builder) where THandler : class, ITerminalExceptionHandler
        {
            // Add exception publisher
            builder.Services.AddTransient<ITerminalExceptionHandler, THandler>();

            return builder;
        }

        /// <summary>
        /// Adds the command <see cref="ITerminalHelpProvider"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddHelpProvider<THelpProvider>(this ITerminalBuilder builder) where THelpProvider : class, ITerminalHelpProvider
        {
            builder.Services.AddSingleton<ITerminalHelpProvider, THelpProvider>();
            return builder;
        }

        /// <summary>
        /// Adds terminal license handler to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddLicensing(this ITerminalBuilder builder)
        {
            // Add license debugger.
            builder.Services.AddSingleton<ILicenseDebugger, LicenseDebugger>();

            // Add license extractor as singleton
            builder.Services.AddSingleton<ILicenseExtractor, LicenseExtractor>();

            // Add license checker as singleton
            builder.Services.AddSingleton<ILicenseChecker, LicenseChecker>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="IDataTypeMapper{TValue}"/> and <see cref="IOptionChecker"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TMapper">The option mapper type.</typeparam>
        /// <typeparam name="TChecker">The option checker type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddOptionChecker<TMapper, TChecker>(this ITerminalBuilder builder) where TMapper : class, IDataTypeMapper<Option> where TChecker : class, IOptionChecker
        {
            builder.Services.AddTransient<IDataTypeMapper<Option>, TMapper>();
            builder.Services.AddTransient<IOptionChecker, TChecker>();
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITerminalProcessor"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <typeparam name="TTerminalProcessor">The terminal processor type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddProcessor<TTerminalProcessor>(this ITerminalBuilder builder)
            where TTerminalProcessor : class, ITerminalProcessor
        {
            builder.Services.AddSingleton<ITerminalProcessor, TTerminalProcessor>();
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITerminalRouter{TContext}"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddTerminalRouter<TRouter, TContext>(this ITerminalBuilder builder) where TRouter : class, ITerminalRouter<TContext> where TContext : TerminalRouterContext
        {
            // Add terminal routing service.
            builder.Services.TryAddSingleton<ITerminalRouter<TContext>, TRouter>();

            return builder;
        }

        /// <summary>
        /// Adds the <see cref="TerminalRouterContext"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="terminalRouterContext">The terminal router context.</param>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        public static ITerminalBuilder AddTerminalRouterContext<TContext>(this ITerminalBuilder builder, TContext terminalRouterContext) where TContext : TerminalRouterContext
        {
            builder.Services.AddSingleton<TerminalRouterContext>(terminalRouterContext);
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="ITerminalTextHandler"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <typeparam name="TTextHandler">The text handler.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        /// <remarks>
        /// <para>
        /// <see cref="AddTextHandler{TTextHandler}(ITerminalBuilder, TTextHandler)"/> requires an instance of
        /// <typeparamref name="TTextHandler"/> instead of just its type because the terminal application is expected to
        /// operate with a single, consistent instance of <see cref="ITerminalTextHandler"/> throughout its lifetime. By
        /// passing an instance, it allows the terminal to maintain state or configuration specific to that instance,
        /// ensuring consistent text handling behavior across different parts of the application.
        /// </para>
        /// <para>
        /// This approach also facilitates more flexible initialization patterns, where the
        /// <see cref="ITerminalTextHandler"/> can be configured or initialized outside of the dependency injection
        /// container before being registered. This can be particularly useful when the text handler requires complex
        /// setup or depends on settings or services that aren't readily available within the DI context.
        /// </para>
        /// </remarks>
        public static ITerminalBuilder AddTextHandler<TTextHandler>(this ITerminalBuilder builder, TTextHandler textHandler) where TTextHandler : class, ITerminalTextHandler
        {
            builder.Services.AddSingleton<ITerminalTextHandler>(textHandler);
            return builder;
        }

        /// <summary>
        /// Starts a new <see cref="ICommandBuilder"/> definition with the default <see cref="CommandChecker"/>.
        /// Applications must call the <see cref="ICommandBuilder.Add"/> method to add the
        /// <see cref="CommandDescriptor"/> to the service collection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="id">The command id.</param>
        /// <param name="name">The command name.</param>
        /// <param name="description">The command description.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="commandFlags">The command flags.</param>
        /// <typeparam name="TRunner">The command runner type.</typeparam>
        /// <returns>The configured <see cref="ITerminalBuilder"/>.</returns>
        /// <returns>The configured <see cref="ICommandBuilder"/>.</returns>
        public static ICommandBuilder DefineCommand<TRunner>(this ITerminalBuilder builder, string id, string name, string description, CommandType commandType) where TRunner : ICommandRunner<CommandRunnerResult>
        {
            return DefineCommand(builder, id, name, description, typeof(CommandChecker), typeof(TRunner), commandType);
        }

        private static ITerminalBuilder AddDeclarativeRunnerInner(this ITerminalBuilder builder, Type declarativeRunner)
        {
            object[] classAttrs = declarativeRunner.GetCustomAttributes(false);

            // Command descriptor The declarative runner is the command runner.
            ICommandDescriptorAttribute cmdAttr = GetDeclarativeInterface<ICommandDescriptorAttribute>(classAttrs) ?? throw new TerminalException(TerminalErrors.InvalidDeclaration, "The declarative target does not define command descriptor.");

            // Get command checker for class level, defaults to CommandChecker if not defined
            Type checkerType = ProcessCommandChecker(classAttrs, typeof(CommandChecker));

            // Establish command builder
            ICommandBuilder commandBuilder = builder.DefineCommand(cmdAttr.Id, cmdAttr.Name, cmdAttr.Description, checkerType, declarativeRunner, cmdAttr.CommandType);

            // Command runner methods
            if (cmdAttr.CommandType == CommandType.CompositeGroup)
            {
                MethodInfo[] runnerMethods = declarativeRunner.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                for (int i = 0; i < runnerMethods.Length; i++)
                {
                    MethodInfo runnerMethod = runnerMethods[i];
                    object[] methodAttrs = runnerMethod.GetCustomAttributes(false);

                    // Get command descriptor attribute from method
                    ICommandDescriptorAttribute? methodCmdAttr = GetDeclarativeInterface<ICommandDescriptorAttribute>(methodAttrs);
                    if (methodCmdAttr == null)
                    {
                        continue;
                    }

                    // Get command checker for method, defaults to CommandChecker if not defined
                    Type methodCheckerType = ProcessCommandChecker(methodAttrs, typeof(CommandChecker));

                    // Create command descriptor for the method
                    ICommandBuilder methodCommandBuilder = builder.DefineCommand(methodCmdAttr.Id, methodCmdAttr.Name, methodCmdAttr.Description, methodCheckerType, declarativeRunner, methodCmdAttr.CommandType);

                    // Process method-level attributes, parent CompositeGroup as owner
                    ProcessCommandAttributes(methodAttrs, methodCommandBuilder, declarativeRunner, null, cmdAttr.Id);

                    // Add the method command descriptor
                    methodCommandBuilder.Add();

                    // Also add the run method to the composite group
                    var runnerbuilder = commandBuilder.DefineRunMethod(cmdAttr.Id, runnerMethod);
                    runnerbuilder.Add();
                }
            }

            // Process class-level attributes
            ProcessCommandAttributes(classAttrs, commandBuilder, null, cmdAttr.CommandType);

            // Build and add the command descriptor to service collection
            return commandBuilder.Add();
        }

        private static void ProcessCommandAttributes(object[] attrs, ICommandBuilder commandBuilder, Type? fallbackAttributeProvider, CommandType? commandType = null, string? parentGroupId = null)
        {
            ProcessArgumentDescriptors(attrs, commandBuilder);
            ProcessOptionDescriptors(attrs, commandBuilder);
            ProcessCustomProperties(attrs, commandBuilder);
            ProcessTags(attrs, commandBuilder);
            ProcessOwners(attrs, commandBuilder, fallbackAttributeProvider, commandType, parentGroupId);
        }

        private static Type ProcessCommandChecker(object[] attrs, Type defaultChecker)
        {
            ICommandCheckerAttribute? cmdChecker = GetDeclarativeInterface<ICommandCheckerAttribute>(attrs);

            return cmdChecker?.Checker ?? defaultChecker;
        }

        private static void ProcessOwners(object[] attrs, ICommandBuilder commandBuilder, Type? fallbackAttributeProvider, CommandType? commandType = null, string? parentGroupId = null)
        {
            ICommandOwnersAttribute? ownersAttr = GetDeclarativeInterface<ICommandOwnersAttribute>(attrs);

            if (ownersAttr != null)
            {
                commandBuilder.Owners(ownersAttr.Owners);
            }
            else if (parentGroupId != null)
            {
                commandBuilder.Owners(new OwnerIdCollection(parentGroupId));
            }
            else if (fallbackAttributeProvider != null)
            {
                object[] fallbackAttrs = fallbackAttributeProvider.GetCustomAttributes(false);

                ICommandOwnersAttribute? fallbackOwnersAttr = GetDeclarativeInterface<ICommandOwnersAttribute>(fallbackAttrs);

                if (fallbackOwnersAttr != null)
                {
                    commandBuilder.Owners(fallbackOwnersAttr.Owners);
                }
            }
            else if (commandType.HasValue && (commandType.Value == CommandType.IsolatedGroup || commandType.Value == CommandType.CompositeGroup || commandType.Value == CommandType.Leaf))
            {
                throw new TerminalException(TerminalErrors.InvalidDeclaration, "The declarative target does not define command owner.");
            }
        }

        private static void ProcessArgumentDescriptors(object[] attrs, ICommandBuilder commandBuilder)
        {
            List<ArgumentDescriptorAttribute> argAttrs = GetDeclarativeInterfaces<ArgumentDescriptorAttribute>(attrs);
            List<ArgumentValidationAttribute> argVdls = GetDeclarativeInterfaces<ArgumentValidationAttribute>(attrs);

            for (int i = 0; i < argAttrs.Count; i++)
            {
                ArgumentDescriptorAttribute argAttr = argAttrs[i];

                IArgumentBuilder argBuilder = commandBuilder.DefineArgument(argAttr.Order, argAttr.Id, argAttr.DataType, argAttr.Description, argAttr.Flags);

                for (int j = 0; j < argVdls.Count; j++)
                {
                    ArgumentValidationAttribute argVdl = argVdls[j];

                    if (argVdl.ArgumentId.Equals(argAttr.Id))
                    {
                        argBuilder.ValidationAttribute(argVdl.ValidationAttribute, argVdl.ValidationParams);
                    }
                }

                argBuilder.Add();
            }
        }

        private static void ProcessOptionDescriptors(object[] attrs, ICommandBuilder commandBuilder)
        {
            List<OptionDescriptorAttribute> optAttrs = GetDeclarativeInterfaces<OptionDescriptorAttribute>(attrs);
            List<OptionValidationAttribute> optVdls = GetDeclarativeInterfaces<OptionValidationAttribute>(attrs);

            for (int i = 0; i < optAttrs.Count; i++)
            {
                OptionDescriptorAttribute optAttr = optAttrs[i];

                IOptionBuilder optBuilder = commandBuilder.DefineOption(optAttr.Id, optAttr.DataType, optAttr.Description, optAttr.Flags, optAttr.Alias);

                for (int j = 0; j < optVdls.Count; j++)
                {
                    OptionValidationAttribute optVdl = optVdls[j];

                    if (optVdl.OptionId.Equals(optAttr.Id))
                    {
                        optBuilder.ValidationAttribute(optVdl.ValidationAttribute, optVdl.ValidationParams);
                    }
                }

                optBuilder.Add();
            }
        }

        private static void ProcessCustomProperties(object[] attrs, ICommandBuilder commandBuilder)
        {
            List<CommandCustomPropertyAttribute> cmdPropAttrs = GetDeclarativeInterfaces<CommandCustomPropertyAttribute>(attrs);

            for (int i = 0; i < cmdPropAttrs.Count; i++)
            {
                CommandCustomPropertyAttribute attr = cmdPropAttrs[i];
                commandBuilder.CustomProperty(attr.Key, attr.Value);
            }
        }

        private static void ProcessTags(object[] attrs, ICommandBuilder commandBuilder)
        {
            CommandTagsAttribute? tagsAttr = GetDeclarativeInterface<CommandTagsAttribute>(attrs);

            if (tagsAttr != null)
            {
                commandBuilder.Tags(tagsAttr.Tags);
            }
        }

        private static ICommandBuilder DefineCommand(this ITerminalBuilder builder, string id, string name, string description, Type checker, Type runner, CommandType commandType)
        {
            CommandDescriptor cmd = new(id, name, description, commandType)
            {
                Checker = checker,
                Runner = runner,
            };

            ICommandBuilder commandBuilder = new CommandBuilder(builder);
            commandBuilder.Services.AddSingleton(cmd);

            return commandBuilder;
        }

        private static TInterface? GetDeclarativeInterface<TInterface>(object[] attrs) where TInterface : class
        {
            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] is TInterface attr)
                {
                    return attr;
                }
            }

            return null;
        }

        private static List<TInterface> GetDeclarativeInterfaces<TInterface>(object[] attrs) where TInterface : class
        {
            List<TInterface> results = new();

            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] is TInterface attr)
                {
                    results.Add(attr);
                }
            }

            return results;
        }
    }
}