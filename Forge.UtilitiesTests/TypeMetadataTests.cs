using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Forge.Utilities.Tests {
    public class TypeMetadataTests {
        private class Simple {
            public int A;
            public int B { get; set; }
            public int No0 { get { return 0; } }
            public int No1 { set { } }
        }

        [Fact]
        public void MetadataPropertiesSimple() {
            TypeMetadata metadata = TypeCache.FindTypeMetadata(typeof(Simple));

            List<string> properties = (from property in metadata.Properties
                                       select property.Name).ToList();

            Assert.Equal(2, properties.Count);
            Assert.Contains("A", properties);
            Assert.Contains("B", properties);
        }

        private class Inherited : Simple {
            public int C;
            public int D { get; set; }
            public int No2 { get { return 0; } }
            public int No3 { set { } }
        }

        [Fact]
        public void MetadataPropertiesInherited() {
            TypeMetadata metadata = TypeCache.FindTypeMetadata(typeof(Inherited));

            List<string> properties = (from property in metadata.Properties
                                       select property.Name).ToList();

            Assert.Equal(4, properties.Count);
            Assert.Contains("A", properties);
            Assert.Contains("B", properties);
            Assert.Contains("C", properties);
            Assert.Contains("D", properties);
        }
    }
}