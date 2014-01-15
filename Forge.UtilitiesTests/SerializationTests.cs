using Newtonsoft.Json;
using System;
using Xunit;

namespace Forge.Utilities.Tests {
    public class SerializationTests {
        private void ValidateType(string serialized) {
            Assert.Equal(typeof(SerializationTests), SerializationHelpers.Deserialize<Type>(serialized));
        }

        [Fact]
        public void RestoreTypeBadAssembly() {
            ValidateType(@"""Forge.Utilities.Tests.SerializationTests""");
            ValidateType(@"""Forge.Utilities.Tests.SerializationTests, Forge.Utilities.Tests""");
            ValidateType(@"""Forge.Utilities.Tests.SerializationTests, bad""");
            ValidateType(@"""Forge.Utilities.Tests.SerializationTests, xunit""");
        }
    }
}