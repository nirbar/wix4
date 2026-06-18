namespace WixToolset.Burn.UnitTest.Internal
{
    using System;
    using System.Text;

    /// <summary>
    /// Cursor-tracked reader for burn's wire-format buffers.
    /// All multi-byte integers are little-endian.
    /// </summary>
    internal sealed class BurnBufferReader
    {
        private readonly byte[] _buffer;
        private int _pos;

        internal BurnBufferReader(byte[] buffer, int startOffset = 0)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _pos = startOffset;
        }

        internal int Position => _pos;

        internal int Remaining => _buffer.Length - _pos;

        internal uint ReadUInt32()
        {
            EnsureRemaining(4);
            var v = BitConverter.ToUInt32(_buffer, _pos);
            _pos += 4;
            return v;
        }

        internal int ReadInt32()
        {
            EnsureRemaining(4);
            var v = BitConverter.ToInt32(_buffer, _pos);
            _pos += 4;
            return v;
        }

        internal ulong ReadUInt64()
        {
            EnsureRemaining(8);
            var v = BitConverter.ToUInt64(_buffer, _pos);
            _pos += 8;
            return v;
        }

        internal bool ReadBool()
        {
            return ReadUInt32() != 0u;
        }

        /// <summary>
        /// Reads a string encoded as [uint32 charCount][charCount * 2 UTF-16LE bytes, no null terminator].
        /// Returns null when charCount is 0xFFFFFFFF (null string sentinel).
        /// </summary>
        internal string ReadString()
        {
            var charCount = ReadUInt32();
            if (charCount == 0xFFFFFFFFu)
            {
                return null;
            }
            if (charCount == 0)
            {
                return string.Empty;
            }
            var byteCount = (int)(charCount * 2);
            EnsureRemaining(byteCount);
            var s = Encoding.Unicode.GetString(_buffer, _pos, byteCount);
            _pos += byteCount;
            return s;
        }

        private void EnsureRemaining(int needed)
        {
            if (_buffer.Length - _pos < needed)
            {
                throw new InvalidOperationException(
                    $"BurnBufferReader underrun: need {needed} bytes at offset {_pos}, buffer length {_buffer.Length}.");
            }
        }
    }
}
