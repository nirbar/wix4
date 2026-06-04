// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest.Internal
{
    internal static class BurnProtocolConstants
    {
        /// <summary>
        /// Message sent by burn to testhost on the .BA pipe indicating it is time to start the real BA.
        /// Value = 0x0FFEFFFF (BOOTSTRAPPER_ENGINE_MESSAGE_UNKNOWN - BOOTSTRAPPER_APPLICATION_MESSAGE_UNKNOWN).
        /// </summary>
        internal const uint BaMessageStartRealBA = 0x0FFEFFFFu;

        /// <summary>
        /// Message sent by testhost to burn on the .BAEngine pipe to stop the restart loop after the last iteration.
        /// Value = 0x0FFFFFFF (BOOTSTRAPPER_ENGINE_MESSAGE_UNKNOWN).
        /// </summary>
        internal const uint EngineMessageQuitAll = 0x0FFFFFFFu;

        /// <summary>
        /// Message sent by testhost to burn on the .BAEngine pipe to quit this test.
        /// </summary>
        internal const uint EngineMessageQuit = 0x17u;

        /// <summary>
        /// Pipe message type indicating client disconnect.
        /// </summary>
        internal const uint PipeMessageDisconnect = 0xFFFFFFFFu;

        /// <summary>
        /// BA protocol API version (WIX_5_BOOTSTRAPPER_APPLICATION_API_VERSION).
        /// </summary>
        internal const uint ApiVersion = 5u;

        /// <summary>
        /// Suffix appended to the base pipe name to form the BA pipe name.
        /// Burn sends BA messages on this pipe; the testhost reads them.
        /// </summary>
        internal const string BaPipeSuffix = ".BA";

        /// <summary>
        /// Suffix appended to the base pipe name to form the BAEngine pipe name.
        /// The testhost can send engine messages to burn on this pipe.
        /// </summary>
        internal const string BAEnginePipeSuffix = ".BAEngine";

        /// <summary>
        /// Timeout in milliseconds for connecting to burn's pipe on the first iteration.
        /// Longer because burn must extract the UX from the bundle.
        /// </summary>
        internal const int PipeFirstConnectTimeoutMs = 600_000; // 10 minutes

        /// <summary>
        /// Timeout in milliseconds for connecting to burn's pipe on subsequent iterations.
        /// Shorter because the UX is already extracted.
        /// </summary>
        internal const int PipeRestartConnectTimeoutMs = 60_000; // 1 minute
    }
}
