namespace WixToolset.Burn.UnitTest.Internal
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Writer for burn's wire-format buffers.
    /// All multi-byte integers are little-endian.
    /// </summary>
    internal sealed class BurnBufferWriter
    {
        private readonly MemoryStream _stream = new MemoryStream();

        internal void WriteUInt32(uint value)
        {
            _stream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        internal void WriteInt32(int value)
        {
            _stream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        internal void WriteUInt64(ulong value)
        {
            _stream.Write(BitConverter.GetBytes(value), 0, 8);
        }

        internal void WriteBool(bool value)
        {
            WriteUInt32(value ? 1u : 0u);
        }

        /// <summary>
        /// Writes a string as [uint32 charCount][charCount * 2 UTF-16LE bytes, no null terminator].
        /// Writes 0xFFFFFFFF for null.
        /// </summary>
        internal void WriteString(string value)
        {
            if (value == null)
            {
                WriteUInt32(0xFFFFFFFFu);
                return;
            }
            WriteUInt32((uint)value.Length);
            if (value.Length > 0)
            {
                var bytes = Encoding.Unicode.GetBytes(value);
                _stream.Write(bytes, 0, bytes.Length);
            }
        }

        internal byte[] ToArray() => _stream.ToArray();
    }
}
