namespace WixToolset.Burn.UnitTest.Internal
{
    using System;
    using System.IO.Pipes;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Server-side named pipes used by the real BA.
    /// The testhost acts as the engine from the real BA's perspective:
    /// the real BA connects to .BA (to receive BA messages from us) and to .BAEngine (to send engine calls to us).
    /// </summary>
    internal sealed class RealBAPipeServer : IDisposable
    {
        private readonly NamedPipeServerStream _baServer;
        private readonly NamedPipeServerStream _baEngineServer;
        private bool _disposed;

        private RealBAPipeServer(NamedPipeServerStream baServer, NamedPipeServerStream baEngineServer)
        {
            _baServer = baServer;
            _baEngineServer = baEngineServer;
        }

        /// <summary>
        /// Creates server-side pipe instances for the real BA to connect to.
        /// </summary>
        internal static RealBAPipeServer Create(string baseName)
        {
            var baServer = new NamedPipeServerStream(
                baseName + BurnProtocolConstants.BaPipeSuffix,
                PipeDirection.InOut,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Byte,
                PipeOptions.None);

            var baEngineServer = new NamedPipeServerStream(
                baseName + BurnProtocolConstants.BAEnginePipeSuffix,
                PipeDirection.InOut,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Byte,
                PipeOptions.None);

            return new RealBAPipeServer(baServer, baEngineServer);
        }

        /// <summary>
        /// Waits for the real BA to connect on both pipes, reads the secret from each client,
        /// verifies it matches <paramref name="expectedSecret"/>, and writes S_OK.
        /// </summary>
        internal async Task WaitForClientConnectAsync(string expectedSecret, CancellationToken ct)
        {
            await _baServer.WaitForConnectionAsync(ct).ConfigureAwait(false);
            var baSecret = BurnPipeIO.ReadSecret(_baServer);
            if (!string.Equals(baSecret, expectedSecret, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Real BA .BA pipe secret mismatch.");
            }
            // Write S_OK
            _baServer.Write(new byte[] { 0, 0, 0, 0 }, 0, 4);

            await _baEngineServer.WaitForConnectionAsync(ct).ConfigureAwait(false);
            var baEngineSecret = BurnPipeIO.ReadSecret(_baEngineServer);
            if (!string.Equals(baEngineSecret, expectedSecret, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Real BA .BAEngine pipe secret mismatch.");
            }
            // Write S_OK
            _baEngineServer.Write(new byte[] { 0, 0, 0, 0 }, 0, 4);
        }

        /// <summary>
        /// Sends a BA message to the real BA: [uint32 msgType][uint32 cbData][data].
        /// </summary>
        internal void SendBAMessage(uint msgType, byte[] payload)
            => BurnPipeIO.WriteMessage(_baServer, msgType, payload);

        /// <summary>
        /// Reads the real BA's RPC response from the .BA pipe.
        /// </summary>
        internal (int hr, byte[] data) ReadBAResponse()
            => BurnPipeIO.ReadResponse(_baServer);

        /// <summary>
        /// Reads one engine message that the real BA sent on the .BAEngine pipe.
        /// </summary>
        internal (uint msgType, byte[] data) ReadEngineMessage()
            => BurnPipeIO.ReadMessage(_baEngineServer);

        /// <summary>
        /// Writes an engine response back to the real BA on the .BAEngine pipe.
        /// </summary>
        internal void WriteEngineResponse(int hr, byte[] data)
            => BurnPipeIO.WriteResponse(_baEngineServer, hr, data);

        /// <summary>
        /// Closes the real BA's engine pipe so it can no longer send engine messages.
        /// Safe to call before <see cref="Dispose"/>; <see cref="System.IO.Stream.Dispose()"/> is idempotent.
        /// </summary>
        internal void DisconnectEnginePipe()
            => _baEngineServer.Dispose();

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _baServer.Dispose();
                _baEngineServer.Dispose();
            }
        }
    }
}
