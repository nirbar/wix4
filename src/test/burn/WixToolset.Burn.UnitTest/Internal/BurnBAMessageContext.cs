namespace WixToolset.Burn.UnitTest.Internal
{
    using System;
    using WixToolset.Burn.UnitTest;

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
        internal void ForwardToRealBA(BurnBATestBase testInstance)
        {
            try
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
            catch (Exception ex)
            {
                testInstance.AddException(ex);
            }
        }

        /// <summary>
        /// Forwards an <c>OnCreate</c> message to the real BA re-serialized from
        /// <paramref name="cmd"/>, allowing test overrides to change command-line flags
        /// (such as <see cref="WixToolset.BootstrapperApplicationApi.LaunchAction"/> or
        /// <see cref="WixToolset.BootstrapperApplicationApi.Display"/>) before the real BA sees them.
        /// Idempotent: subsequent calls return the cached response.
        /// </summary>
        internal void ForwardOnCreateToRealBA(BurnBATestBase testInstance, TestBaCommand cmd)
        {
            try
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

                // Re-serialize the (possibly modified) command into the wire format that the real BA expects.
                var argsWriter = new BurnBufferWriter();
                argsWriter.WriteUInt32(BurnProtocolConstants.ApiVersion);
                argsWriter.WriteUInt32(0u);                          // cbSize — unused by managed BAs
                argsWriter.WriteUInt32((uint)cmd.Action);
                argsWriter.WriteUInt32((uint)cmd.Display);
                argsWriter.WriteString(cmd.CommandLine);
                argsWriter.WriteInt32(cmd.CmdShow);
                argsWriter.WriteUInt32((uint)cmd.Resume);
                argsWriter.WriteUInt64(0UL);                         // hwndSplashScreen
                argsWriter.WriteUInt32((uint)cmd.Relation);
                argsWriter.WriteBool(cmd.Passthrough);
                argsWriter.WriteString(cmd.LayoutDirectory);
                argsWriter.WriteString(cmd.BootstrapperWorkingFolder);
                argsWriter.WriteString(cmd.BootstrapperApplicationDataPath);

                var argsBytes = argsWriter.ToArray();
                var defaultResults = this.ExtractDefaultResultsBytes();

                // Reconstruct full payload: [cbArgs][argsBytes][cbResults][defaultResultsBytes]
                var newPayload = new byte[4 + argsBytes.Length + 4 + defaultResults.Length];
                Buffer.BlockCopy(BitConverter.GetBytes((uint)argsBytes.Length), 0, newPayload, 0, 4);
                Buffer.BlockCopy(argsBytes, 0, newPayload, 4, argsBytes.Length);
                Buffer.BlockCopy(BitConverter.GetBytes((uint)defaultResults.Length), 0, newPayload, 4 + argsBytes.Length, 4);
                Buffer.BlockCopy(defaultResults, 0, newPayload, 4 + argsBytes.Length + 4, defaultResults.Length);

                this.RealBA.SendBAMessage(this.MessageType, newPayload);
                var (hr, data) = this.RealBA.ReadBAResponse();
                this.ResponseHr = hr;
                this.ResponseData = data ?? Array.Empty<byte>();
                this.WasForwarded = true;
            }
            catch (Exception ex)
            {
                testInstance.AddException(ex);
            }
        }

        private byte[] ExtractDefaultResultsBytes()
        {
            var raw = this.RawPayload;
            if (raw.Length < 4)
            {
                return Array.Empty<byte>();
            }
            var cbArgs = (int)BitConverter.ToUInt32(raw, 0);
            var offset = 4 + cbArgs;
            if (raw.Length < offset + 4)
            {
                return Array.Empty<byte>();
            }
            var cbResults = (int)BitConverter.ToUInt32(raw, offset);
            offset += 4;
            if (raw.Length < offset + cbResults)
            {
                return Array.Empty<byte>();
            }
            var result = new byte[cbResults];
            Buffer.BlockCopy(raw, offset, result, 0, cbResults);
            return result;
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
