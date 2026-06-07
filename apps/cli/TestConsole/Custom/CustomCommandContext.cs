using System.Collections.Generic;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Apps.Test.Custom
{
    public class CustomCommandContext(Dictionary<string, object> properties) : CommandContext(properties)
    {
    }
}