using Forge.Entities.Implementation.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Forge.Entities.Tests {
    public class DataReferenceTests {
        private static DataReference<TData> CreateDataReference<TData>(IQueryableEntity entity)
            where TData : IData {
            var dataReference = new DataReference<TData>();
            ((IDataReference)dataReference).Provider = entity;
            return dataReference;
        }

        [Fact]
        public void DataReference() {
            IEntity entity = new ContentEntity(0, "");
            entity.AddData<DataInt>().A = 1;

            DataReference<DataInt> reference = CreateDataReference<DataInt>(entity);
            Assert.Equal(1, reference.Current().A);
            Assert.Equal(1, reference.Current<DataInt>().A);
            Assert.Throws<InvalidOperationException>(() => reference.Current<DataEmpty>());
        }
    }
}