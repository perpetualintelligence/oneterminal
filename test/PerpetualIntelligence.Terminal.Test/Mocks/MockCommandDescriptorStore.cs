﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Stores;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockCommandDescriptorStore : ICommandStoreHandler
    {
        public Task<ReadOnlyDictionary<string, CommandDescriptor>> AllAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<CommandDescriptor> FindByIdAsync(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}