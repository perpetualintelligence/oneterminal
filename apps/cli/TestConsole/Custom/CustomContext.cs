using System.Collections.Generic;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Apps.Test.Custom
{
    public class CustomContext(Dictionary<string, object> properties) : ICommandContext
    {
        public Dictionary<string, object> Properties { get; } = properties;
    }
}