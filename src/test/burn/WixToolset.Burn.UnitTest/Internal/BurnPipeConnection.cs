namespace WixToolset.Burn.UnitTest.Internal
{
    using System;
    using System.IO.Pipes;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Client-side connection to burn's .BA and .BAEngine named pipe servers.
    /// Burn creates the servers; the testhost connects as client.
    /// </summary>
    internal sealed class BurnPipeConnection : IDisposable
    {
        private readonly NamedPipeClientStream _baPipe;
        private readonly NamedPipeClientStream _baEnginePipe;
        private bool _disposed;

        private BurnPipeConnection(NamedPipeClientStream baPipe, NamedPipeClientStream baEnginePipe)
        {
            _baPipe = baPipe;
            _baEnginePipe = baEnginePipe;
            this.EnginePipeLock = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Guards access to the .BAEngine pipe between the engine relay task and the quit sender.
        /// </summary>
        internal SemaphoreSlim EnginePipeLock { get; }

        /// <summary>
        /// Connects to burn's .BA and .BAEngine pipes, sends the secret handshake on each.
        /// </summary>
        internal static async Task<BurnPipeConnection> ConnectAsync(
            string baseName,
            string secret,
            bool firstIteration,
            CancellationToken ct)
        {
            var timeoutMs = firstIteration
                ? BurnProtocolConstants.PipeFirstConnectTimeoutMs
                : BurnProtocolConstants.PipeRestartConnectTimeoutMs;

            var baPipe = new NamedPipeClientStream(
                ".", baseName + BurnProtocolConstants.BaPipeSuffix,
                PipeDirection.InOut, PipeOptions.None);

            var baEnginePipe = new NamedPipeClientStream(
                ".", baseName + BurnProtocolConstants.BAEnginePipeSuffix,
                PipeDirection.InOut, PipeOptions.None);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);

            await baPipe.ConnectAsync(cts.Token).ConfigureAwait(false);
            BurnPipeIO.SendSecret(baPipe, secret);

            await baEnginePipe.ConnectAsync(cts.Token).ConfigureAwait(false);
            BurnPipeIO.SendSecret(baEnginePipe, secret);

            return new BurnPipeConnection(baPipe, baEnginePipe);
        }

        /// <summary>
        /// Reads one BA message from burn: [uint32 msgType][uint32 cbData][data].
        /// </summary>
        internal (uint msgType, byte[] payload) ReadBAMessage()
            => BurnPipeIO.ReadMessage(_baPipe);

        /// <summary>
        /// Writes an RPC response back to burn on the .BA pipe: [uint32 hr][uint32 cbData][data].
        /// </summary>
        internal void WriteBAResponse(int hr, byte[] data)
            => BurnPipeIO.WriteResponse(_baPipe, hr, data);

        /// <summary>
        /// Reads one engine message that the real BA sent on the .BAEngine pipe (used by relay task).
        /// </summary>
        internal (int hr, byte[] data) ReadEngineResponse()
            => BurnPipeIO.ReadResponse(_baEnginePipe);

        /// <summary>
        /// Sends an engine message to burn's .BAEngine pipe (used for EngineMessageQuit).
        /// Caller must hold <see cref="EnginePipeLock"/>.
        /// </summary>
        internal void SendEngineMessage(uint msgType, byte[] data)
            => BurnPipeIO.WriteMessage(_baEnginePipe, msgType, data);

        /// <summary>
        /// Writes an engine response to burn's .BAEngine pipe (used by relay task).
        /// Caller must hold <see cref="EnginePipeLock"/>.
        /// </summary>
        internal void WriteEngineResponse(int hr, byte[] data)
            => BurnPipeIO.WriteResponse(_baEnginePipe, hr, data);

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                this.EnginePipeLock.Dispose();
                _baPipe.Dispose();
                _baEnginePipe.Dispose();
            }
        }
    }
}
