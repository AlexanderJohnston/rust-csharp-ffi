using System;
using Db.Storage.Native;

namespace Db.Storage
{
    public sealed class Reader : IDisposable
    {
        private readonly ReaderHandle _handle;

        internal Reader(ReaderHandle handle)
        {
            _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        }

        public void Dispose()
        {
            _handle.Dispose();
        }

        public ReadResult TryReadNext(Span<byte> buffer)
        {
            unsafe
            {
                fixed (byte* bufferPtr = buffer)
                {
                    var result = Bindings.db_read_next(
                        _handle,
                        out var key,
                        (IntPtr) bufferPtr,
                        (UIntPtr) buffer.Length,
                        out var actualValueLength);

                    if (result.IsBufferTooSmall()) return ReadResult.BufferTooSmall((int) actualValueLength);

                    if (result.IsDone()) return ReadResult.Done();

                    return ReadResult.Data(new Key(key), buffer, ..(int) actualValueLength);
                }
            }
        }
    }
}