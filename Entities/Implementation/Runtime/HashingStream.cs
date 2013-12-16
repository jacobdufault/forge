using System;
using System.IO;
using System.Security.Cryptography;

namespace Neon.Entities.Implementation.Runtime {
    /// <summary>
    /// This class was taken from http://stackoverflow.com/a/5378007
    /// </summary>
    internal class HashingStream : Stream {
        protected readonly HashAlgorithm HashAlgorithm;

        protected HashingStream(HashAlgorithm hash) {
            HashAlgorithm = hash;
        }

        public static byte[] GetHash<T>(T obj, HashAlgorithm hash) {
            var hasher = new HashingStream(hash);

            if (obj != null) {
                ProtoBuf.Serializer.Serialize(hasher, obj);
                hasher.Flush();
            }
            else {
                hasher.Flush();
            }

            return hasher.HashAlgorithm.Hash;
        }

        public override bool CanRead {
            get { throw new NotImplementedException(); }
        }

        public override bool CanSeek {
            get { throw new NotImplementedException(); }
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override void Flush() {
            HashAlgorithm.TransformFinalBlock(new byte[0], 0, 0);
        }

        public override long Length {
            get { throw new NotImplementedException(); }
        }

        public override long Position {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            HashAlgorithm.TransformBlock(buffer, offset, count, buffer, offset);
        }
    }
}