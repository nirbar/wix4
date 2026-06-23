// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest.Mimic
{
    using WixToolset.BootstrapperApplicationApi;

    /// <summary>
    /// Represents one MSI message to fire via <see cref="IBootstrapperApplication.OnExecuteMsiMessage"/>
    /// during the simulated execute phase in mimic-engine mode.
    /// Populate <see cref="BurnBATestBase.MimicMsiMessages"/> with instances of this class
    /// to assert how the BA reacts to MSI progress or status messages.
    /// </summary>
    public sealed class MimicMsiMessage
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MimicMsiMessage"/>.
        /// </summary>
        /// <param name="messageType">The MSI message type (e.g. <see cref="InstallMessage.ActionStart"/>).</param>
        /// <param name="message">Human-readable message text.</param>
        /// <param name="uiFlags">
        /// MSI UI hint flags (MB_OK etc.).  Typically 0 for progress messages.
        /// </param>
        public MimicMsiMessage(InstallMessage messageType, string message, int uiFlags = 0)
        {
            this.MessageType = messageType;
            this.Message = message ?? string.Empty;
            this.UiFlags = uiFlags;
        }

        /// <summary>Gets the MSI message type.</summary>
        public InstallMessage MessageType { get; }

        /// <summary>Gets the message text.</summary>
        public string Message { get; }

        /// <summary>Gets the MSI UI hint flags.</summary>
        public int UiFlags { get; }
    }
}
