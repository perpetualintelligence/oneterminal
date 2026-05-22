//  Copyright © 2019-2026 Perpetual Intelligence L.L.C. All rights reserved.
//  For license, terms, and data policies, go to:
//  https://terms.perpetualintelligence.com/articles/intro.html

using System.Collections.ObjectModel;

namespace OneImlx.Terminal.Shared
{
    /// <summary>
    /// A keyed collection by id.
    /// </summary>
    /// <typeparam name="TItem">The collection item type.</typeparam>
    public class KeyAsIdCollection<TItem> : KeyedCollection<string, TItem> where TItem : IKeyAsId
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        public KeyAsIdCollection(ITerminalTextHandler textHandler) : base(textHandler.EqualityComparer())
        {
        }

        /// <summary>
        /// Gets the key for the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected override string GetKeyForItem(TItem item)
        {
            return item.CommandId;
        }
    }
}