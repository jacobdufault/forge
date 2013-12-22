// The MIT License (MIT)
//
// Copyright (c) 2013 Jacob Dufault
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neon.Entities.Implementation.Content;
using Neon.Entities.Implementation.Runtime;
using Neon.Utilities;
using System;
using System.Collections.Generic;

namespace Neon.Entities.Tests {

    internal class TestData0 : BaseData<TestData0> {
        public override bool SupportsConcurrentModifications {
            get { return false; }
        }

        public override void CopyFrom(TestData0 source) {
        }
    }

    internal class TestData1 : BaseData<TestData1> {
        public override bool SupportsConcurrentModifications {
            get { return false; }
        }

        public override void CopyFrom(TestData1 source) {
        }
    }

    internal static class EntityHelpers {
        private static UniqueIntGenerator _idGenerator = new UniqueIntGenerator();

        public static RuntimeEntity CreateEntity() {
            RuntimeEntity runtime = new RuntimeEntity();
            runtime.Initialize(new ContentEntitySerializationFormat() {
                Data = new List<ContentEntity.DataInstance>(),
                PrettyName = "",
                UniqueId = _idGenerator.Next()
            });
            return runtime;
        }
    }

    [TestClass]
    public class EntityTest {
        [TestMethod]
        public void EntityCreation() {
            IEntity entity = EntityHelpers.CreateEntity();
        }

        [TestMethod]
        public void EntityUniqueId() {
            HashSet<int> ids = new HashSet<int>();

            for (int i = 0; i < 200; ++i) {
                IEntity entity = EntityHelpers.CreateEntity();
                Assert.IsTrue(ids.Add(entity.UniqueId));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(AlreadyAddedDataException))]
        public void CannotAddMultipleDataInstances() {
            RuntimeEntity entity = EntityHelpers.CreateEntity();
            entity.AddData<TestData0>();
            entity.AddData<TestData0>();
            entity.DataStateChangeUpdate();
        }

        [TestMethod]
        [ExpectedException(typeof(AlreadyAddedDataException))]
        public void CannotAddMultipleDataInstancesAfterStateUpdates() {
            RuntimeEntity entity = EntityHelpers.CreateEntity();
            entity.AddData<TestData0>();
            entity.DataStateChangeUpdate();
            entity.AddData<TestData0>();
            entity.DataStateChangeUpdate();
        }

        [TestMethod]
        public void InitializingReturnsOneConstantInstance() {
            IEntity entity = EntityHelpers.CreateEntity();
            TestData0 data0 = entity.AddData<TestData0>();
            TestData0 data1 = entity.AddOrModify<TestData0>();
            TestData0 data2 = entity.Modify<TestData0>();

            Assert.AreEqual(data0, data1);
            Assert.AreEqual(data0, data2);
        }

        [TestMethod]
        [ExpectedException(typeof(NoSuchDataException))]
        public void AddedDataIsNotContained() {
            IEntity entity = EntityHelpers.CreateEntity();
            entity.AddData<TestData0>();

            Assert.IsFalse(entity.ContainsData<TestData0>());
            entity.Current<TestData0>();
        }

        [TestMethod]
        public void RemovedDataIsNotReturnedInSelectData() {
            RuntimeEntity entity = EntityHelpers.CreateEntity();
            entity.AddData<TestData0>();
            entity.DataStateChangeUpdate();
            Assert.AreEqual(1, entity.SelectData().Count);

            entity.RemoveData<TestData0>();
            entity.DataStateChangeUpdate();

            Assert.AreEqual(0, entity.SelectData().Count);
        }

        /*
        [TestMethod]
        public void RemovedDataIsAccessibleThroughCurrent() {
            IEntity entity = new Entity();
            entity.AddData<TestData0>();

            ((Entity)entity).DataStateChangeUpdate();
            Assert.IsTrue(entity.ContainsData<TestData0>());

            entity.RemoveData<TestData0>();
            ((Entity)entity).DataStateChangeUpdate();

            entity.Current<TestData0>(); // ensure we can access the current data
            Assert.IsFalse(entity.ContainsData<TestData0>());
            try {
                entity.Previous<TestData0>();
                Assert.Fail();
            }
            catch (NoSuchDataException) { }
            try {
                entity.Modify<TestData0>();
                Assert.Fail();
            }
            catch (NoSuchDataException) { }

        }
        */
    }
}