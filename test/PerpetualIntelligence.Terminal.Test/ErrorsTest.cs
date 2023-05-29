﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Test.Services;

namespace PerpetualIntelligence.Terminal
{
    [TestClass]
    public class ErrorsTest
    {
        [TestMethod]
        public void AssertErrorsAreValid()
        {
            TestHelper.AssertConstantCount(typeof(Errors), 15);

            Assert.AreEqual("connection_closed", Errors.ConnectionClosed);
            Assert.AreEqual("invalid_command", Errors.InvalidCommand);
            Assert.AreEqual("invalid_configuration", Errors.InvalidConfiguration);
            Assert.AreEqual("invalid_option", Errors.InvalidOption);
            Assert.AreEqual("duplicate_option", Errors.DuplicateOption);
            Assert.AreEqual("invalid_request", Errors.InvalidRequest);
            Assert.AreEqual("unsupported_option", Errors.UnsupportedOption);
            Assert.AreEqual("unsupported_command", Errors.UnsupportedCommand);
            Assert.AreEqual("server_error", Errors.ServerError);
            Assert.AreEqual("missing_option", Errors.MissingOption);
            Assert.AreEqual("missing_claim", Errors.MissingClaim);
            Assert.AreEqual("request_canceled", Errors.RequestCanceled);
            Assert.AreEqual("invalid_license", Errors.InvalidLicense);
            Assert.AreEqual("unauthorized_access", Errors.UnauthorizedAccess);
            Assert.AreEqual("invalid_declaration", Errors.InvalidDeclaration);
        }
    }
}