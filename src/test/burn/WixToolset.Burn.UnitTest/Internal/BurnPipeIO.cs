// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest.Internal
{
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Text;

    /// <summary>
    /// Static helpers for reading and writing burn pipe messages and RPC responses.
    /// </summary>
    internal static class BurnPipeIO
    {
        /// <summary>
        /// Writes a pipe message: [uint32 msgType][uint32 cbData][data].
        /// </summary>
        internal static void WriteMessage(PipeStream pipe, uint msgType, byte[] data)
        {
            var header = new byte[8];
            BitConverter.TryWriteBytes(new Span<byte>(header, 0, 4), msgType);
            BitConverter.TryWriteBytes(new Span<byte>(header, 4, 4), (uint)(data?.Length ?? 0));
            pipe.Write(header, 0, 8);
            if (data != null && data.Length > 0)
            {
                pipe.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Reads a pipe message: [uint32 msgType][uint32 cbData][data].
        /// Returns (PipeMessageDisconnect, empty) when the pipe is closed.
        /// </summary>
        internal static (uint msgType, byte[] data) ReadMessage(PipeStream pipe)
        {
            var header = new byte[8];
            if (!ReadExact(pipe, header, 8))
            {
                return (BurnProtocolConstants.PipeMessageDisconnect, Array.Empty<byte>());
            }
            var msgType = BitConverter.ToUInt32(header, 0);
            var cbData = BitConverter.ToUInt32(header, 4);
            if (cbData == 0)
            {
                return (msgType, Array.Empty<byte>());
            }
            var data = new byte[cbData];
            if (!ReadExact(pipe, data, (int)cbData))
            {
                return (BurnProtocolConstants.PipeMessageDisconnect, Array.Empty<byte>());
            }
            return (msgType, data);
        }

        /// <summary>
        /// Writes an RPC response: [uint32 hr][uint32 cbData][data].
        /// </summary>
        internal static void WriteResponse(PipeStream pipe, int hr, byte[] data)
        {
            var header = new byte[8];
            BitConverter.TryWriteBytes(new Span<byte>(header, 0, 4), hr);
            BitConverter.TryWriteBytes(new Span<byte>(header, 4, 4), (uint)(data?.Length ?? 0));
            pipe.Write(header, 0, 8);
            if (data != null && data.Length > 0)
            {
                pipe.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Reads an RPC response: [uint32 hr][uint32 cbData][data].
        /// </summary>
        internal static (int hr, byte[] data) ReadResponse(PipeStream pipe)
        {
            var header = new byte[8];
            if (!ReadExact(pipe, header, 8))
            {
                throw new IOException("Pipe closed while reading RPC response header.");
            }
            var hr = BitConverter.ToInt32(header, 0);
            var cbData = BitConverter.ToUInt32(header, 4);
            if (cbData == 0)
            {
                return (hr, Array.Empty<byte>());
            }
            var data = new byte[cbData];
            if (!ReadExact(pipe, data, (int)cbData))
            {
                throw new IOException("Pipe closed while reading RPC response body.");
            }
            return (hr, data);
        }

        /// <summary>
        /// Sends the client secret handshake: [uint32 cbBytes][UTF-16LE bytes].
        /// Then reads the server's [uint32 hrResult] and throws if it is not S_OK.
        /// </summary>
        internal static void SendSecret(PipeStream pipe, string secret)
        {
            var secretBytes = Encoding.Unicode.GetBytes(secret);
            var cbHeader = new byte[4];
            BitConverter.TryWriteBytes(cbHeader, (uint)secretBytes.Length);
            pipe.Write(cbHeader, 0, 4);
            pipe.Write(secretBytes, 0, secretBytes.Length);

            var hrBytes = new byte[4];
            if (!ReadExact(pipe, hrBytes, 4))
            {
                throw new IOException("Pipe closed while reading secret handshake response.");
            }
            var hrResult = BitConverter.ToInt32(hrBytes, 0);
            if (hrResult != 0)
            {
                throw new InvalidOperationException($"Pipe secret handshake failed with HRESULT 0x{hrResult:X8}.");
            }
        }

        /// <summary>
        /// Reads the client secret handshake: [uint32 cbBytes][UTF-16LE bytes].
        /// Returns the decoded secret string.
        /// </summary>
        internal static string ReadSecret(PipeStream pipe)
        {
            var cbHeader = new byte[4];
            if (!ReadExact(pipe, cbHeader, 4))
            {
                throw new IOException("Pipe closed while reading secret byte count.");
            }
            var cbBytes = BitConverter.ToUInt32(cbHeader, 0);
            if (cbBytes == 0)
            {
                return string.Empty;
            }
            var secretBytes = new byte[cbBytes];
            if (!ReadExact(pipe, secretBytes, (int)cbBytes))
            {
                throw new IOException("Pipe closed while reading secret bytes.");
            }
            return Encoding.Unicode.GetString(secretBytes);
        }

        /// <summary>
        /// Reads exactly <paramref name="count"/> bytes from the pipe into <paramref name="buffer"/>.
        /// Returns false if the pipe is closed before all bytes are available.
        /// </summary>
        private static bool ReadExact(PipeStream pipe, byte[] buffer, int count)
        {
            var offset = 0;
            while (offset < count)
            {
                var read = pipe.Read(buffer, offset, count - offset);
                if (read == 0)
                {
                    return false;
                }
                offset += read;
            }
            return true;
        }
    }
}
