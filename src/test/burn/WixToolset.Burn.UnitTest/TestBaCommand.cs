// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using WixToolset.BootstrapperApplicationApi;

    /// <summary>
    /// Provides readable, mutable access to the bundle command-line parameters that are sent to
    /// <see cref="IBootstrapperApplication.OnCreate"/> by the burn engine.
    ///
    /// Because the underlying <see cref="Command"/> struct has <c>private readonly</c> fields that
    /// cannot be set from outside the API assembly, test authors should use this class — accessible
    /// via <see cref="BurnBATestBase.Command"/> — instead of the <c>ref Command</c> parameter
    /// passed to <see cref="BurnBATestBase.OnCreate"/>.
    /// </summary>
    public sealed class TestBaCommand
    {
        internal TestBaCommand(
            LaunchAction action,
            Display display,
            string commandLine,
            int cmdShow,
            ResumeType resume,
            RelationType relation,
            bool passthrough,
            string layoutDirectory,
            string bootstrapperWorkingFolder,
            string bootstrapperApplicationDataPath)
        {
            this.Action = action;
            this.Display = display;
            this.CommandLine = commandLine;
            this.CmdShow = cmdShow;
            this.Resume = resume;
            this.Relation = relation;
            this.Passthrough = passthrough;
            this.LayoutDirectory = layoutDirectory;
            this.BootstrapperWorkingFolder = bootstrapperWorkingFolder;
            this.BootstrapperApplicationDataPath = bootstrapperApplicationDataPath;
        }

        /// <summary>Gets or sets the requested bundle action.</summary>
        public LaunchAction Action { get; set; }

        /// <summary>Gets or sets the requested display mode.</summary>
        public Display Display { get; set; }

        /// <summary>Gets or sets the full command line passed to the bundle.</summary>
        public string CommandLine { get; set; }

        /// <summary>Gets or sets the SW_* show command for the splash screen window.</summary>
        public int CmdShow { get; set; }

        /// <summary>Gets or sets the resume type (normal, reboot, suspend, etc.).</summary>
        public ResumeType Resume { get; set; }

        /// <summary>Gets or sets the relationship type when the bundle is being launched by a related bundle.</summary>
        public RelationType Relation { get; set; }

        /// <summary>Gets or sets whether the bundle is running in passthrough mode.</summary>
        public bool Passthrough { get; set; }

        /// <summary>Gets or sets the layout directory, if any.</summary>
        public string LayoutDirectory { get; set; }

        /// <summary>Gets or sets the working folder for the bootstrapper.</summary>
        public string BootstrapperWorkingFolder { get; set; }

        /// <summary>Gets or sets the path to the bootstrapper application data file.</summary>
        public string BootstrapperApplicationDataPath { get; set; }

        /// <summary>
        /// Builds a <see cref="Command"/> struct with the values from this instance.
        /// Unmanaged string memory is allocated for each non-null string field and added to
        /// <paramref name="pinnedStrings"/> so the caller can free it after use.
        /// </summary>
        internal Command ToCommand(List<IntPtr> pinnedStrings)
        {
            var boxed = (object)default(Command);
            var t = typeof(Command);

            SetField(boxed, t, "cbSize", Marshal.SizeOf<Command>());
            SetField(boxed, t, "action", this.Action);
            SetField(boxed, t, "display", this.Display);
            SetField(boxed, t, "scope", default(BundleScope));
            SetField(boxed, t, "wzCommandLine", PinString(this.CommandLine, pinnedStrings));
            SetField(boxed, t, "nCmdShow", this.CmdShow);
            SetField(boxed, t, "resume", this.Resume);
            SetField(boxed, t, "hwndSplashScreen", IntPtr.Zero);
            SetField(boxed, t, "relation", this.Relation);
            SetField(boxed, t, "passthrough", this.Passthrough);
            SetField(boxed, t, "wzLayoutDirectory", PinString(this.LayoutDirectory, pinnedStrings));
            SetField(boxed, t, "wzBootstrapperWorkingFolder", PinString(this.BootstrapperWorkingFolder, pinnedStrings));
            SetField(boxed, t, "wzBootstrapperApplicationDataPath", PinString(this.BootstrapperApplicationDataPath, pinnedStrings));

            return (Command)boxed;
        }

        private static IntPtr PinString(string value, List<IntPtr> pinnedStrings)
        {
            if (value == null)
            {
                return IntPtr.Zero;
            }

            var ptr = Marshal.StringToHGlobalUni(value);
            pinnedStrings.Add(ptr);
            return ptr;
        }

        private static void SetField(object boxed, Type type, string name, object value)
        {
            var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(boxed, value);
        }
    }
}
