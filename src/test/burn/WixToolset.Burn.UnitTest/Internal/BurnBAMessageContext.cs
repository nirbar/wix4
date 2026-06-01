// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest.Internal
{
    using System;

    /// <summary>
    /// Per-dispatch context set on <see cref="BurnBATestBase"/> before each BA message call.
    /// Holds the raw payload from burn and optionally the forwarded response from the real BA.
    /// </summary>
    internal sealed class BurnBAMessageContext
    {
        internal BurnBAMessageContext(uint messageType, byte[] rawPayload, RealBAPipeServer realBA)
        {
            this.MessageType = messageType;
            this.RawPayload = rawPayload ?? Array.Empty<byte>();
            this.RealBA = realBA;
        }

        internal uint MessageType { get; }

        /// <summary>
        /// Raw [cbArgs][argsBytes][cbResults][defaultResultsBytes] payload from burn.
        /// </summary>
        internal byte[] RawPayload { get; }

        internal RealBAPipeServer RealBA { get; }

        internal bool WasForwarded { get; private set; }

        internal int ResponseHr { get; private set; }

        internal byte[] ResponseData { get; private set; } = Array.Empty<byte>();

        /// <summary>
        /// Forwards the current message to the real BA and caches its response.
        /// Idempotent: subsequent calls return the cached response.
        /// </summary>
        internal void ForwardToRealBA()
        {
            if (this.WasForwarded)
            {
                return;
            }
            if (this.RealBA == null)
            {
                throw new InvalidOperationException(
                    "Cannot forward to real BA: no real BA server is available for this iteration.");
            }
            this.RealBA.SendBAMessage(this.MessageType, this.RawPayload);
            var (hr, data) = this.RealBA.ReadBAResponse();
            this.ResponseHr = hr;
            this.ResponseData = data ?? Array.Empty<byte>();
            this.WasForwarded = true;
        }

        /// <summary>
        /// Returns a reader over the args bytes section of the payload.
        /// Payload layout: [uint32 cbArgs][argsBytes][uint32 cbResults][defaultResultsBytes]
        /// </summary>
        internal BurnBufferReader GetArgsReader()
        {
            if (this.RawPayload.Length < 4)
            {
                return new BurnBufferReader(Array.Empty<byte>());
            }
            var cbArgs = BitConverter.ToUInt32(this.RawPayload, 0);
            return new BurnBufferReader(this.RawPayload, startOffset: 4);
            // Caller reads up to cbArgs bytes from offset 4.
            // We pass the whole buffer; the caller is responsible for reading the right count.
        }

        /// <summary>
        /// Returns a reader over the default results bytes section of the payload.
        /// Used to initialise ref-parameter defaults before calling the virtual method.
        /// </summary>
        internal BurnBufferReader GetDefaultResultsReader()
        {
            if (this.RawPayload.Length < 4)
            {
                return new BurnBufferReader(Array.Empty<byte>());
            }
            var cbArgs = (int)BitConverter.ToUInt32(this.RawPayload, 0);
            var resultsOffset = 4 + cbArgs;
            if (this.RawPayload.Length < resultsOffset + 4)
            {
                return new BurnBufferReader(Array.Empty<byte>());
            }
            // cbResults is at resultsOffset; actual results bytes start 4 bytes after that.
            return new BurnBufferReader(this.RawPayload, startOffset: resultsOffset + 4);
        }
    }
}
