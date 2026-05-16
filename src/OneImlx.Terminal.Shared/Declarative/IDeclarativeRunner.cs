//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

namespace OneImlx.Terminal.Shared.Declarative
{
    /// <summary>
    /// Specifies a runner that provides declarative command and option descriptors.
    /// </summary>
    /// <remarks>
    /// The DI engine uses reflection to identify all the declarative runners and populate the command and
    /// option descriptors.
    /// </remarks>
    public interface IDeclarativeRunner
    {
    }
}