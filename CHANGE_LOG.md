# 5.1.1-rc*
> Breaking Change
- Rename `OptionValueWithIn` to `ValueDelimiter` in extractor options
- Remove default option and default option value feature in favor of command argument feature
- Remove `IErrorHandler` in favor of `IExceptionHandler`
- Rename `IRoutingService` to `ITerminalRouting`
- Move routing service to runtime namespace
- Introduce ITerminalConsole abstraction
- Rename Errors, Handlers
- Remove ConsoleHelper

# 5.0.1-rc*
> Breaking Change
- Remove the default option feature
- Introduce the command argument feature
- Introduce the terminal driver feature 
- Enforce terminal identifier

# 4.5.2-rc*
> Breaking Change
- Change the default values for `OptionPrefix`, `OptionAliasPrefix`, and `OptionValueSeparator`
- Initial support for on-premise licensing 

# 4.5.1-rc*
> Breaking Change
- Rename `Cli` namespace to `Terminal`
- Rename `CliOptions` to `TerminalOptions`
- Rename `*CliBuilder` to `*TerminalBuilder`
- Rename CICI actions for `PerpetualIntelligence.Cli` to `PerpetualIntelligence.Terminal`
- Deprecate `PerpetualIntelligence.Cli` in favor of `PerpetualIntelligence.Terminal`

# 4.4.2-rc*
> Breaking Change
- Rename all classes with `Cli*` to `Terminal*`
- Rename all classes with `Argument*` to `Option*`
- Rename Router Remote Options
- Check maximum command string length in router
- Update Demo license schema

# v4.3.2-rc*
- Add TCP IP routing service
- Port old code base and migrate to .NET Standard2.0, ,NET Standard2.1 and xUnit Tests for .NET7