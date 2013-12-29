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

using Forge.Entities.Implementation.Content;
using Forge.Entities.Implementation.Runtime;
using Forge.Entities.Implementation.Shared;
using Forge.Utilities;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;

namespace Forge.Entities.Tests {
    internal static class EntityTestHelpers {
        private static UniqueIntGenerator _idGenerator = new UniqueIntGenerator();

        public static ContentEntity CreateContentEntity() {
            ContentEntity entity = new ContentEntity();
            entity.Initialize(new ContentEntitySerializationFormat() {
                Data = new List<ContentEntity.DataInstance>(),
                PrettyName = "",
                UniqueId = _idGenerator.Next()
            });
            return entity;
        }

        public static RuntimeEntity CreateRuntimeEntity() {
            RuntimeEntity entity = new RuntimeEntity();
            entity.Initialize(new ContentEntitySerializationFormat() {
                Data = new List<ContentEntity.DataInstance>(),
                PrettyName = "",
                UniqueId = _idGenerator.Next()
            }, new EventNotifier());
            return entity;
        }
    }

    public class EntityTest {
        [Fact]
        public void EntityUniqueId() {
            HashSet<int> ids = new HashSet<int>();

            IGameSnapshot snapshot = LevelManager.CreateSnapshot();

            for (int i = 0; i < 200; ++i) {
                IEntity entity = snapshot.CreateEntity();
                Assert.True(ids.Add(entity.UniqueId));
            }
        }

        public static IEnumerable<object[]> EmptyEntityTypes {
            get {
                return new List<object[]>() {
                    new object[] { EntityTestHelpers.CreateRuntimeEntity() },
                    new object[] { EntityTestHelpers.CreateContentEntity() }
                };
            }
        }

        [Theory, PropertyData("EmptyEntityTypes")]
        public void CannotAddMultipleDataInstances(IEntity entity) {
            entity.AddData<DataEmpty>();
            Assert.Throws<AlreadyAddedDataException>(() => entity.AddData<DataEmpty>());
        }

        [Fact]
        public void InitializingReturnsOneConstantInstanceRuntime() {
            IEntity entity = EntityTestHelpers.CreateRuntimeEntity();
            DataEmpty data0 = entity.AddData<DataEmpty>();
            DataEmpty data1 = entity.AddOrModify<DataEmpty>();
            DataEmpty data2 = entity.Modify<DataEmpty>();

            Assert.ReferenceEquals(data0, data1);
            Assert.ReferenceEquals(data0, data2);
        }

        [Fact]
        public void AddedDataIsNotContainedRuntime() {
            IEntity entity = EntityTestHelpers.CreateRuntimeEntity();
            entity.AddData<DataEmpty>();

            Assert.False(entity.ContainsData<DataEmpty>());
            Assert.Throws<NoSuchDataException>(() => entity.Current<DataEmpty>());
        }

        [Fact]
        public void RemovedDataIsNotReturnedInSelectData() {
            RuntimeEntity entity = EntityTestHelpers.CreateRuntimeEntity();
            entity.AddData<DataEmpty>();
            entity.DataStateChangeUpdate();
            Assert.Equal(1, entity.SelectData().Count);

            entity.RemoveData<DataEmpty>();
            Assert.Equal(1, entity.SelectData().Count);

            entity.DataStateChangeUpdate();
            Assert.Equal(0, entity.SelectData().Count);
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