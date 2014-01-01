using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Forge.Utilities.Tests {
    public class UniqueIntGeneratorTests {
        [Fact]
        public void UniqueIntGeneratorConsumeStartsAtNextInt() {
            var gen = new UniqueIntGenerator();
            gen.Consume(5);
            Assert.Equal(6, gen.Next());

            gen.Consume(3);
            Assert.Equal(7, gen.Next());

            gen.Consume(7);
            Assert.Equal(8, gen.Next());

            gen.Consume(9);
            Assert.Equal(10, gen.Next());

            gen.Consume(-32);
            gen.Consume(-35);
            gen.Consume(3);
            gen.Consume(-10000);
            Assert.Equal(11, gen.Next());
        }

        [Fact]
        public void UniqueIntGeneratorStartsAt0() {
            var gen = new UniqueIntGenerator();
            Assert.Equal(0, gen.Next());
        }
    }
}