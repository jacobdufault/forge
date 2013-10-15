using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;

namespace SerializationTests {
    interface IMessage {
    }
    [Serializable]
    class Message0 : IMessage {
        public int a;
    }
    [Serializable]
    class Message00 : Message0 {
        public int b;
    }
    [Serializable]
    struct Message1 : IMessage {
        public int a, b, c;
    }
    [Serializable]
    struct Message2 : IMessage {
        public int a, b, c, d;
        public IMessage message;
    }

    [Serializable]
    struct ListMessage {
        public List<int> items;
    }

    [TestClass]
    public class SerializationTests {
        [TestMethod]
        public void Serialization() {
            NetSerializer.Serializer.Initialize(
                typeof(Message0),
                typeof(Message00),
                typeof(Message1),
                typeof(Message2),
                typeof(ListMessage));

            using (MemoryStream stream = new MemoryStream()) {
                Message0 m0 = new Message0() {
                    a = 0
                };
                Message00 m00 = new Message00() {
                    a = 1, b = 2
                };
                Message1 m1 = new Message1() {
                    a = 3, b = 4, c = 5
                };
                Message2 m2 = new Message2() {
                    a = 6, b = 7, c = 8, d = 9, message = m00
                };

                NetSerializer.Serializer.Serialize(stream, m0);
                NetSerializer.Serializer.Serialize(stream, m00);
                NetSerializer.Serializer.Serialize(stream, m1);
                NetSerializer.Serializer.Serialize(stream, m2);
                stream.Seek(0, SeekOrigin.Begin);


                Message0 d0 = NetSerializer.Serializer.Deserialize<Message0>(stream);
                Message00 d00 = NetSerializer.Serializer.Deserialize<Message00>(stream);
                Message1 d1 = NetSerializer.Serializer.Deserialize<Message1>(stream);
                Message2 d2 = NetSerializer.Serializer.Deserialize<Message2>(stream);

                Assert.AreEqual(m0.a, d0.a);
                Assert.AreEqual(m00.a, d00.a);
                Assert.AreEqual(m00.b, d00.b);
                Assert.AreEqual(m1, d1);
                Assert.AreEqual(m2.a, d2.a);
                Assert.AreEqual(m2.b, d2.b);
                Assert.AreEqual(m2.c, d2.c);
                Assert.AreEqual(m2.d, d2.d);
                Assert.AreEqual(((Message00)m2.message).a, ((Message00)d2.message).a);
                Assert.AreEqual(((Message00)m2.message).b, ((Message00)d2.message).b);

                /////////////////////////////////////////////////////

                ListMessage lm0 = new ListMessage() {
                    items = null
                };
                ListMessage lm1 = new ListMessage() {
                    items = new List<int>()
                };
                ListMessage lm2 = new ListMessage() {
                    items = new List<int>()
                };
                lm2.items.Add(1);
                lm2.items.Add(2);
                lm2.items.Add(3);
                lm2.items.Add(4);

                stream.Seek(0, SeekOrigin.Begin);
                NetSerializer.Serializer.Serialize(stream, lm0);
                NetSerializer.Serializer.Serialize(stream, lm1);
                NetSerializer.Serializer.Serialize(stream, lm2);
                
                stream.Seek(0, SeekOrigin.Begin);
                ListMessage dm0 = NetSerializer.Serializer.Deserialize<ListMessage>(stream);
                ListMessage dm1 = NetSerializer.Serializer.Deserialize<ListMessage>(stream);
                ListMessage dm2 = NetSerializer.Serializer.Deserialize<ListMessage>(stream);

                Assert.AreEqual(lm0.items, dm0.items);
                CollectionAssert.AreEqual(lm1.items, dm1.items);
                CollectionAssert.AreEqual(lm2.items, dm2.items);
            }
        }
    }
}
