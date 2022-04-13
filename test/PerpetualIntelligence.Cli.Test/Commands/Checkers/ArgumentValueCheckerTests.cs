﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Shared.Attributes.Validation;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    [TestClass]
    public class ArgumentValueCheckerTests : InitializerTests
    {
        public ArgumentValueCheckerTests() : base(TestLogger.Create<ArgumentValueCheckerTests>())
        {
        }

        [TestMethod]
        public async Task StrictTypeChecking_InvalidMappedType_ButConvertible_ShouldNotErrorAsync()
        {
            options.Checker.StrictTypeChecking = true;

            // Value is double, but we can convert it so this should not error.
            ArgumentDescriptor identity = new("arg1", DataType.Text);
            Argument value = new("arg1", 23.69, DataType.Text);

            ArgumentCheckerContext context = new(identity, value);
            await checker.CheckAsync(context);

            // Check converted
            Assert.AreEqual("23.69", value.Value);
            Assert.IsInstanceOfType(value.Value, typeof(string));
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabled_InvalidMappedType_ButConvertible_ShouldNotErrorAsync()
        {
            options.Checker.StrictTypeChecking = false;

            // Value is double, strict checking is disabled so we will not convert it
            ArgumentDescriptor identity = new("arg1", DataType.Text);
            Argument value = new("arg1", 23.69, DataType.Text);

            ArgumentCheckerContext context = new(identity, value);
            await checker.CheckAsync(context);

            // Check not converted
            Assert.AreEqual(23.69, value.Value);
            Assert.IsInstanceOfType(value.Value, typeof(double));
        }

        [TestMethod]
        public async Task MapperFailureShouldErrorAsync()
        {
            // Any failure, we just want to test that mapper failure is correclty returned
            ArgumentDescriptor identity = new("arg1", (DataType)int.MaxValue);
            Argument value = new("arg1", 23.69, (DataType)int.MaxValue);

            ArgumentCheckerContext context = new (identity, value);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.UnsupportedArgument, "The argument data type is not supported. argument=arg1 data_type=2147483647");
        }

        [TestMethod]
        public async Task NullArgumentValueShouldErrorAsync()
        {
            ArgumentDescriptor identity = new("arg1", DataType.Text);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Argument value = new("arg1", null, DataType.Text);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            ArgumentCheckerContext context = new (identity, value);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument value cannot be null. argument=arg1");
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabledNotSupportedValueShouldNotErrorAsync()
        {
            options.Checker.StrictTypeChecking = false;

            ArgumentDescriptor identity = new("arg1", DataType.Text, validationAttributes: new[] { new OneOfAttribute("test1", "test2") });
            Argument value = new("arg1", "test3", DataType.Text);

            ArgumentCheckerContext context = new(identity, value);
            await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task StrictTypeCheckingNotSupportedValueShouldErrorAsync()
        {
            options.Checker.StrictTypeChecking = true;

            ArgumentDescriptor identity = new("arg1", DataType.Text, validationAttributes: new[] { new OneOfAttribute("test1", "test2") });
            Argument value = new("arg1", "test3", DataType.Text);

            ArgumentCheckerContext context = new(identity, value);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument value is not valid. argument=arg1 value=test3 additional_info=The field value must be one of the valid values.");
        }

        [TestMethod]
        public async Task SupportedValueShouldNotErrorAsync()
        {
            ArgumentDescriptor identity = new("arg1", DataType.Text, validationAttributes: new[] { new OneOfAttribute("test1", "test2") });
            Argument value = new("arg1", "test2", DataType.Text);

            ArgumentCheckerContext context = new(identity, value);
            ArgumentCheckerResult result = await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task StrictTypeCheckingSystemTypeMatchAndDataValidationFailShouldErrorAsync()
        {
            options.Checker.StrictTypeChecking = true;

            ArgumentDescriptor identity = new("arg1", DataType.CreditCard, validationAttributes: new[] { new CreditCardAttribute() });
            Argument value = new("arg1", "invalid_4242424242424242", DataType.CreditCard);

            ArgumentCheckerContext context = new ArgumentCheckerContext(identity, value);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => checker.CheckAsync(context), Errors.InvalidArgument, "The argument value is not valid. argument=arg1 value=invalid_4242424242424242 additional_info=The Argument field is not a valid credit card number.");
        }

        [TestMethod]
        public async Task StrictTypeCheckingDisabledSystemTypeMatchAndDataValidationFailShouldNotErrorAsync()
        {
            options.Checker.StrictTypeChecking = false;

            ArgumentDescriptor identity = new("arg1", DataType.CreditCard, validationAttributes: new[] { new CreditCardAttribute() });
            Argument value = new("arg1", "invalid_4242424242424242", DataType.CreditCard);

            ArgumentCheckerContext context = new ArgumentCheckerContext(identity, value);
            await checker.CheckAsync(context);
        }

        [TestMethod]
        public async Task SystemTypeMatchAndDataValidationSuccessShouldNotErrorAsync()
        {
            ArgumentDescriptor identity = new("arg1", DataType.CreditCard);
            Argument value = new("arg1", "4242424242424242", DataType.CreditCard);

            ArgumentCheckerContext context = new ArgumentCheckerContext(identity, value);
            var result = await checker.CheckAsync(context);
            Assert.AreEqual(typeof(string), result.MappedType);
        }

        [TestMethod]
        public void ValidationAttributeCheckFailShouldError()
        {
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.New();
            mapper = new DataAnnotationsArgumentDataTypeMapper(options, TestLogger.Create<DataAnnotationsArgumentDataTypeMapper>());
            checker = new ArgumentChecker(mapper, options, TestLogger.Create<ArgumentChecker>());
        }

        private IArgumentChecker checker = null!;
        private IArgumentDataTypeMapper mapper = null!;
        private CliOptions options = null!;
    }
}
