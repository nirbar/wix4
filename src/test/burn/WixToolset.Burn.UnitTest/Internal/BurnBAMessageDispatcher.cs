// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest.Internal
{
    using System;
    using System.Collections.Generic;
    using WixToolset.BootstrapperApplicationApi;

    /// <summary>
    /// BOOTSTRAPPER_APPLICATION_MESSAGE values from BootstrapperApplicationTypes.h.
    /// UNKNOWN = 65536 (0x10000); named values are UNKNOWN + offset.
    /// </summary>
    internal enum BurnApplicationMessage : uint
    {
        BOOTSTRAPPER_APPLICATION_MESSAGE_UNKNOWN = 0x10000u,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCREATE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDESTROY,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONSTARTUP,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONSHUTDOWN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTFORWARDCOMPATIBLEBUNDLE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTMSIFEATURE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTPACKAGEBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTPACKAGECOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTPATCHTARGET,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTRELATEDBUNDLE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTRELATEDMSIPACKAGE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTUPDATEBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTUPDATE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTUPDATECOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANMSIFEATURE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANPACKAGEBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANPACKAGECOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANPATCHTARGET,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANRELATEDBUNDLE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONAPPLYBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONELEVATBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONELEVATECOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPROGRESS,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONERROR,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONREGISTERBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONREGISTERCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEPACKAGEBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEACQUIREBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEACQUIREPROGRESS,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEACQUIRERESOLVING,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEACQUIRECOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEVERIFYBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEVERIFYCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEPACKAGECOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHECOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEPACKAGEBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEPATCHTARGET,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEPROGRESS,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEMSIMESSAGE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEFILESINUSE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONEMBEDDEDCUSTOMMESSAGE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEPACKAGECOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTECOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONUNREGISTERBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONUNREGISTERCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONAPPLYCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONLAUNCHAPPROVEDEXEBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONLAUNCHAPPROVEDEXECOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANMSIPACKAGE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONBEGINMSITRANSACTIONBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONBEGINMSITRANSACTIONCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCOMMITMSITRANSACTIONBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCOMMITMSITRANSACTIONCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONROLLBACKMSITRANSACTIONBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONROLLBACKMSITRANSACTIONCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPAUSEAUTOMATICUPDATESBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPAUSEAUTOMATICUPDATESCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONSYSTEMRESTOREPOINTBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONSYSTEMRESTOREPOINTCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANNEDPACKAGE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANFORWARDCOMPATIBLEBUNDLE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEVERIFYPROGRESS,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHECONTAINERORPAYLOADVERIFYBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHECONTAINERORPAYLOADVERIFYCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHECONTAINERORPAYLOADVERIFYPROGRESS,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEPAYLOADEXTRACTBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEPAYLOADEXTRACTCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEPAYLOADEXTRACTPROGRESS,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANMSITRANSACTION,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANMSITRANSACTIONCOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTCOMPATIBLEMSIPACKAGE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANCOMPATIBLEMSIPACKAGEBEGIN,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANCOMPATIBLEMSIPACKAGECOMPLETE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANNEDCOMPATIBLEPACKAGE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANRESTORERELATEDBUNDLE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANRELATEDBUNDLETYPE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONAPPLYDOWNGRADE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEPROCESSCANCEL,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTRELATEDBUNDLEPACKAGE,
        BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEPACKAGENONVITALVALIDATIONFAILURE,
    }

    /// <summary>
    /// Deserializes BA message payloads and dispatches them to the appropriate virtual method on
    /// a <see cref="BurnBATestBase"/> instance.  Serializes the (possibly overridden) response back.
    /// </summary>
    internal static class BurnBAMessageDispatcher
    {
        /// <summary>
        /// Dispatches one BA message to the test instance.
        /// </summary>
        /// <param name="instance">The test instance to dispatch to.</param>
        /// <param name="msgType">The message type from burn.</param>
        /// <param name="payload">Raw [cbArgs][argsBytes][cbResults][defaultResultsBytes] payload.</param>
        /// <param name="realBA">Active real BA pipe server, or null if there is no real BA this iteration.</param>
        /// <returns>(hr, responseBytes) to be written back to burn.</returns>
        internal static (int hr, byte[] responseData) Dispatch(
            BurnBATestBase instance,
            uint msgType,
            byte[] payload,
            RealBAPipeServer realBA)
        {
            // Update phase-tracking flags before dispatch so autopilot checks see correct state.
            TrackPhase(instance, (BurnApplicationMessage)msgType);

            var ctx = new BurnBAMessageContext(msgType, payload, realBA);
            instance._messageContext = ctx;

            try
            {
                return DispatchCore(instance, ctx, (BurnApplicationMessage)msgType);
            }
            finally
            {
                instance._messageContext = null;
            }
        }

        /// <summary>
        /// Updates phase-tracking state on <paramref name="instance"/> before the message is
        /// dispatched, so autopilot and cancel-guard checks see the correct phase.
        /// </summary>
        private static void TrackPhase(BurnBATestBase instance, BurnApplicationMessage msg)
        {
            if (msg == BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONAPPLYBEGIN)
            {
                instance._applyBeginSeen = true;
            }
            else if (msg == BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONAPPLYCOMPLETE)
            {
                instance._applyCompleteSeen = true;
            }
        }

        /// <summary>
        /// Invokes a BA callback with test-failure guarding:
        /// <list type="bullet">
        ///   <item>If the test is already in a failed state the call is skipped entirely (the
        ///   caller will use the locally-declared default values for all output parameters,
        ///   which keeps the pipe from hanging and lets subsequent messages cancel burn).</item>
        ///   <item>If the call throws â€” including <see cref="OperationCanceledException"/>
        ///   which has no special meaning in a burn BA lifecycle â€” the exception is recorded as
        ///   <see cref="BurnBATestBase.TestFailureException"/> (first failure wins) and
        ///   <see cref="BurnBATestBase.Dispose"/> is called for test-class cleanup, then 0 is
        ///   returned so the caller can build a valid response from its default values.</item>
        /// </list>
        /// </summary>
        private static int CallDispatch(BurnBATestBase instance, Func<int> action)
        {
            if (instance.TestFailureException != null || instance.EndTestAutoPilot)
            {
                // Already failed or autopilot active- skip the override and let the caller return cancel defaults.
                return 0;
            }

            try
            {
                return action();
            }
            catch (Exception ex)
            {
                instance.SetException(ex);
                TryDispose(instance);
                return 0;
            }
        }

        private static void TryDispose(BurnBATestBase instance)
        {
            try { instance.Dispose(); }
            catch { /* never mask the original test failure */ }
        }

        private static (int hr, byte[] responseData) DispatchCore(
            BurnBATestBase instance,
            BurnBAMessageContext ctx,
            BurnApplicationMessage msg)
        {
            switch (msg)
            {
                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCREATE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    a.ReadUInt32(); // cbSize (struct size, unused in managed code)
                    var action = (LaunchAction)a.ReadUInt32();
                    var display = (Display)a.ReadUInt32();
                    var commandLine = a.ReadString();
                    var nCmdShow = a.ReadInt32();
                    var resumeType = (ResumeType)a.ReadUInt32();
                    a.ReadUInt64(); // hwndSplashScreen (unused in managed code; IntPtr.Zero passed below)
                    var relationType = (RelationType)a.ReadUInt32();
                    var fPassthrough = a.ReadBool();
                    var layoutDirectory = a.ReadString();
                    var bootstrapperWorkingFolder = a.ReadString();
                    var bootstrapperApplicationDataPath = a.ReadString();

                    // Build the TestBaCommand wrapper so test authors can inspect command-line values.
                    var testCommand = new TestBaCommand(
                        action,
                        display,
                        commandLine,
                        nCmdShow,
                        resumeType,
                        relationType,
                        fPassthrough,
                        layoutDirectory,
                        bootstrapperWorkingFolder,
                        bootstrapperApplicationDataPath);
                    instance.Command = testCommand;

                    // Build the Command struct for the virtual method signature.
                    // String fields in Command are IntPtr; we pin them for the duration of this call.
                    var pinnedStrings = new List<IntPtr>();
                    try
                    {
                        var cmd = testCommand.ToCommand(pinnedStrings);
                        int hr = CallDispatch(instance, () => instance.OnCreate(null!, ref cmd));
                        if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                        var w = new BurnBufferWriter();
                        w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                        return (hr, w.ToArray());
                    }
                    finally
                    {
                        foreach (var ptr in pinnedStrings) { System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr); }
                    }
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDESTROY:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var reload = a.ReadBool();

                    int hr = CallDispatch(instance, () => instance.OnDestroy(reload));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONSTARTUP:
                {
                    int hr = CallDispatch(instance, () => instance.OnStartup());
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONSHUTDOWN:
                {
                    // Read default action from initial results
                    var dr = ctx.GetDefaultResultsReader();
                    dr.ReadUInt32(); // apiVersion
                    var action = (BOOTSTRAPPER_SHUTDOWN_ACTION)(dr.Remaining >= 4 ? dr.ReadUInt32() : 0u);

                    int hr = CallDispatch(instance, () => instance.OnShutdown(ref action));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)action);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var registrationType = (RegistrationType)a.ReadUInt32();
                    var cPackages = a.ReadInt32();
                    var fCached = a.ReadBool();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnDetectBegin(fCached, registrationType, cPackages, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var hrStatus = a.ReadInt32();
                    var fEligibleForCleanup = a.ReadBool();

                    int hr = CallDispatch(instance, () => instance.OnDetectComplete(hrStatus, fEligibleForCleanup));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTFORWARDCOMPATIBLEBUNDLE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var bundleCode = a.ReadString()!;
                    var relationType = (RelationType)a.ReadUInt32();
                    var bundleTag = a.ReadString()!;
                    var fPerMachine = a.ReadBool();
                    var version = a.ReadString()!;
                    var fMissingFromCache = a.ReadBool();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnDetectForwardCompatibleBundle(bundleCode, relationType, bundleTag, fPerMachine, version, fMissingFromCache, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTUPDATEBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var updateLocation = a.ReadString()!;
                    bool fCancel = false;
                    bool fSkip = false;

                    int hr = CallDispatch(instance, () => instance.OnDetectUpdateBegin(updateLocation, ref fCancel, ref fSkip));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    w.WriteBool(fSkip);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTUPDATE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var updateLocation = a.ReadString()!;
                    var dw64Size = (long)a.ReadUInt64();
                    var hash = a.ReadString()!;
                    var hashAlgorithm = (UpdateHashType)a.ReadUInt32();
                    var version = a.ReadString()!;
                    var title = a.ReadString()!;
                    var summary = a.ReadString()!;
                    var contentType = a.ReadString()!;
                    var content = a.ReadString()!;
                    bool fCancel = false;
                    bool fStopProcessingUpdates = false;

                    int hr = CallDispatch(instance, () => instance.OnDetectUpdate(updateLocation, dw64Size, hash, hashAlgorithm, version, title, summary, contentType, content, ref fCancel, ref fStopProcessingUpdates));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    w.WriteBool(fStopProcessingUpdates);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTUPDATECOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var hrStatus = a.ReadInt32();
                    bool fIgnoreError = false;

                    int hr = CallDispatch(instance, () => instance.OnDetectUpdateComplete(hrStatus, ref fIgnoreError));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fIgnoreError);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTRELATEDBUNDLE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var bundleCode = a.ReadString()!;
                    var relationType = (RelationType)a.ReadUInt32();
                    var bundleTag = a.ReadString()!;
                    var fPerMachine = a.ReadBool();
                    var version = a.ReadString()!;
                    var fMissingFromCache = a.ReadBool();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnDetectRelatedBundle(bundleCode, relationType, bundleTag, fPerMachine, version, fMissingFromCache, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTPACKAGEBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnDetectPackageBegin(packageId, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTPACKAGECOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var hrStatus = a.ReadInt32();
                    var state = (PackageState)a.ReadUInt32();
                    var fCached = a.ReadBool();

                    int hr = CallDispatch(instance, () => instance.OnDetectPackageComplete(packageId, hrStatus, state, fCached));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTPATCHTARGET:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var productCode = a.ReadString()!;
                    var patchState = (PackageState)a.ReadUInt32();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnDetectPatchTarget(packageId, productCode, patchState, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTRELATEDMSIPACKAGE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var upgradeCode = a.ReadString()!;
                    var productCode = a.ReadString()!;
                    var fPerMachine = a.ReadBool();
                    var version = a.ReadString()!;
                    var operation = (RelatedOperation)a.ReadUInt32();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnDetectRelatedMsiPackage(packageId, upgradeCode, productCode, fPerMachine, version, operation, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTMSIFEATURE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var featureId = a.ReadString()!;
                    var state = (FeatureState)a.ReadUInt32();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnDetectMsiFeature(packageId, featureId, state, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTCOMPATIBLEMSIPACKAGE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var compatiblePackageId = a.ReadString()!;
                    var compatiblePackageVersion = a.ReadString()!;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnDetectCompatibleMsiPackage(packageId, compatiblePackageId, compatiblePackageVersion, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTRELATEDBUNDLEPACKAGE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var bundleCode = a.ReadString()!;
                    var relationType = (RelationType)a.ReadUInt32();
                    var fPerMachine = a.ReadBool();
                    var version = a.ReadString()!;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnDetectRelatedBundlePackage(packageId, bundleCode, relationType, fPerMachine, version, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                // -----------------------------------------------------------------------
                // Plan phase
                // -----------------------------------------------------------------------

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var cPackages = a.ReadInt32();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnPlanBegin(cPackages, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var hrStatus = a.ReadInt32();

                    int hr = CallDispatch(instance, () => instance.OnPlanComplete(hrStatus));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANRELATEDBUNDLE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var bundleCode = a.ReadString()!;
                    var recommendedState = (RequestState)a.ReadUInt32();
                    var pRequestedState = recommendedState;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnPlanRelatedBundle(bundleCode, recommendedState, ref pRequestedState, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    w.WriteUInt32((uint)pRequestedState);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANPACKAGEBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var state = (PackageState)a.ReadUInt32();
                    var fCached = a.ReadBool();
                    var installCondition = (BOOTSTRAPPER_PACKAGE_CONDITION_RESULT)a.ReadUInt32();
                    var repairCondition = (BOOTSTRAPPER_PACKAGE_CONDITION_RESULT)a.ReadUInt32();
                    var recommendedState = (RequestState)a.ReadUInt32();
                    var recommendedCacheType = (BOOTSTRAPPER_CACHE_TYPE)a.ReadUInt32();
                    var pRequestedState = recommendedState;
                    var pRequestedCacheType = recommendedCacheType;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnPlanPackageBegin(packageId, state, fCached, installCondition, repairCondition, recommendedState, recommendedCacheType, ref pRequestedState, ref pRequestedCacheType, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    w.WriteUInt32((uint)pRequestedState);
                    w.WriteUInt32((uint)pRequestedCacheType);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANPACKAGECOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var hrStatus = a.ReadInt32();
                    var requested = (RequestState)a.ReadUInt32();

                    int hr = CallDispatch(instance, () => instance.OnPlanPackageComplete(packageId, hrStatus, requested));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANPATCHTARGET:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var productCode = a.ReadString()!;
                    var recommendedState = (RequestState)a.ReadUInt32();
                    var pRequestedState = recommendedState;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnPlanPatchTarget(packageId, productCode, recommendedState, ref pRequestedState, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    w.WriteUInt32((uint)pRequestedState);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANMSIFEATURE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var featureId = a.ReadString()!;
                    var recommendedState = (FeatureState)a.ReadUInt32();
                    var pRequestedState = recommendedState;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnPlanMsiFeature(packageId, featureId, recommendedState, ref pRequestedState, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)pRequestedState);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANMSIPACKAGE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var fExecute = a.ReadBool();
                    var action = (ActionState)a.ReadUInt32();
                    var recommendedFileVersioning = (BOOTSTRAPPER_MSI_FILE_VERSIONING)a.ReadUInt32();
                    bool fCancel = false;
                    var actionMsiProperty = BURN_MSI_PROPERTY.None;
                    var uiLevel = INSTALLUILEVEL.NoChange;
                    bool fDisableExternalUiHandler = false;
                    var fileVersioning = recommendedFileVersioning;

                    int hr = CallDispatch(instance, () => instance.OnPlanMsiPackage(packageId, fExecute, action, recommendedFileVersioning, ref fCancel, ref actionMsiProperty, ref uiLevel, ref fDisableExternalUiHandler, ref fileVersioning));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    w.WriteUInt32((uint)actionMsiProperty);
                    w.WriteUInt32((uint)uiLevel);
                    w.WriteBool(fDisableExternalUiHandler);
                    w.WriteUInt32((uint)fileVersioning);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANCOMPATIBLEMSIPACKAGEBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var compatiblePackageId = a.ReadString()!;
                    var compatiblePackageVersion = a.ReadString()!;
                    var fRecommendedRemove = a.ReadBool();
                    bool fRequestRemove = fRecommendedRemove;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnPlanCompatibleMsiPackageBegin(packageId, compatiblePackageId, compatiblePackageVersion, fRecommendedRemove, ref fRequestRemove, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    w.WriteBool(fRequestRemove);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANCOMPATIBLEMSIPACKAGECOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var compatiblePackageId = a.ReadString()!;
                    var hrStatus = a.ReadInt32();
                    var fRequestedRemove = a.ReadBool();

                    int hr = CallDispatch(instance, () => instance.OnPlanCompatibleMsiPackageComplete(packageId, compatiblePackageId, hrStatus, fRequestedRemove));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANNEDPACKAGE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var execute = (ActionState)a.ReadUInt32();
                    var rollback = (ActionState)a.ReadUInt32();
                    var fPlannedCache = a.ReadBool();
                    var fPlannedUncache = a.ReadBool();

                    int hr = CallDispatch(instance, () => instance.OnPlannedPackage(packageId, execute, rollback, fPlannedCache, fPlannedUncache));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANNEDCOMPATIBLEPACKAGE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var compatiblePackageId = a.ReadString()!;
                    var fRemove = a.ReadBool();

                    int hr = CallDispatch(instance, () => instance.OnPlannedCompatiblePackage(packageId, compatiblePackageId, fRemove));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANFORWARDCOMPATIBLEBUNDLE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var bundleCode = a.ReadString()!;
                    var relationType = (RelationType)a.ReadUInt32();
                    var bundleTag = a.ReadString()!;
                    var fPerMachine = a.ReadBool();
                    var version = a.ReadString()!;
                    var fRecommendedIgnoreBundle = a.ReadBool();
                    bool fCancel = false;
                    bool fIgnoreBundle = fRecommendedIgnoreBundle;

                    int hr = CallDispatch(instance, () => instance.OnPlanForwardCompatibleBundle(bundleCode, relationType, bundleTag, fPerMachine, version, fRecommendedIgnoreBundle, ref fCancel, ref fIgnoreBundle));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    w.WriteBool(fIgnoreBundle);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANRESTORERELATEDBUNDLE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var bundleCode = a.ReadString()!;
                    var recommendedState = (RequestState)a.ReadUInt32();
                    var pRequestedState = recommendedState;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnPlanRestoreRelatedBundle(bundleCode, recommendedState, ref pRequestedState, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    w.WriteUInt32((uint)pRequestedState);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANRELATEDBUNDLETYPE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var bundleCode = a.ReadString()!;
                    var recommendedType = (RelatedBundlePlanType)a.ReadUInt32();
                    var pRequestedType = recommendedType;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnPlanRelatedBundleType(bundleCode, recommendedType, ref pRequestedType, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    w.WriteUInt32((uint)pRequestedType);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANMSITRANSACTION:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var transactionId = a.ReadString()!;
                    bool fTransaction = false;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnPlanMsiTransaction(transactionId, ref fTransaction, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fTransaction);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANMSITRANSACTIONCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var transactionId = a.ReadString()!;
                    var dwPackagesInTransaction = a.ReadUInt32();
                    var fPlanned = a.ReadBool();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnPlanMsiTransactionComplete(transactionId, dwPackagesInTransaction, fPlanned, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                // -----------------------------------------------------------------------
                // Apply phase
                // -----------------------------------------------------------------------

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONAPPLYBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var dwPhaseCount = a.ReadInt32();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnApplyBegin(dwPhaseCount, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONELEVATBEGIN:
                {
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnElevateBegin(ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONELEVATECOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var hrStatus = a.ReadInt32();

                    int hr = CallDispatch(instance, () => instance.OnElevateComplete(hrStatus));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPROGRESS:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var dwProgressPercentage = a.ReadInt32();
                    var dwOverallPercentage = a.ReadInt32();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnProgress(dwProgressPercentage, dwOverallPercentage, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONERROR:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var errorType = (ErrorType)a.ReadUInt32();
                    var packageId = a.ReadString()!;
                    var dwCode = a.ReadInt32();
                    var wzError = a.ReadString()!;
                    var dwUIHint = a.ReadInt32();
                    var cData = (int)a.ReadUInt32();
                    var rgwzData = new string[cData];
                    for (int i = 0; i < cData; i++) { rgwzData[i] = a.ReadString()!; }
                    var nRecommendation = (Result)a.ReadInt32();
                    var pResult = nRecommendation;

                    int hr = CallDispatch(instance, () => instance.OnError(errorType, packageId, dwCode, wzError, dwUIHint, cData, rgwzData, nRecommendation, ref pResult));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteInt32((int)pResult);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONREGISTERBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var recommendedRegistrationType = (RegistrationType)a.ReadUInt32();
                    bool fCancel = false;
                    var pRegistrationType = recommendedRegistrationType;

                    int hr = CallDispatch(instance, () => instance.OnRegisterBegin(recommendedRegistrationType, ref fCancel, ref pRegistrationType));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    w.WriteUInt32((uint)pRegistrationType);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONREGISTERCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var hrStatus = a.ReadInt32();

                    int hr = CallDispatch(instance, () => instance.OnRegisterComplete(hrStatus));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                // -----------------------------------------------------------------------
                // Cache phase
                // -----------------------------------------------------------------------

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEBEGIN:
                {
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnCacheBegin(ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEPACKAGEBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var cCachePayloads = a.ReadInt32();
                    var dw64PackageCacheSize = (long)a.ReadUInt64();
                    var fVital = a.ReadBool();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnCachePackageBegin(packageId, cCachePayloads, dw64PackageCacheSize, fVital, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEACQUIREBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageOrContainerId = a.ReadString()!;
                    var payloadId = a.ReadString()!;
                    var source = a.ReadString()!;
                    var downloadUrl = a.ReadString()!;
                    var payloadContainerId = a.ReadString()!;
                    var recommendation = (CacheOperation)a.ReadUInt32();
                    var action = recommendation;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnCacheAcquireBegin(packageOrContainerId, payloadId, source, downloadUrl, payloadContainerId, recommendation, ref action, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    w.WriteUInt32((uint)action);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEACQUIREPROGRESS:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageOrContainerId = a.ReadString()!;
                    var payloadId = a.ReadString()!;
                    var dw64Progress = (long)a.ReadUInt64();
                    var dw64Total = (long)a.ReadUInt64();
                    var dwOverallPercentage = a.ReadInt32();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnCacheAcquireProgress(packageOrContainerId, payloadId, dw64Progress, dw64Total, dwOverallPercentage, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEACQUIRERESOLVING:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageOrContainerId = a.ReadString()!;
                    var payloadId = a.ReadString()!;
                    var cSearchPaths = (int)a.ReadUInt32();
                    var searchPaths = new string[cSearchPaths];
                    for (int i = 0; i < cSearchPaths; i++) { searchPaths[i] = a.ReadString()!; }
                    var fFoundLocal = a.ReadBool();
                    var fVital = a.ReadBool();
                    var dwRecommendedSearchPath = a.ReadInt32();
                    var wzDownloadUrl = a.ReadString()!;
                    var wzPayloadContainerId = a.ReadString()!;
                    var recommendation = (CacheResolveOperation)a.ReadUInt32();
                    var dwChosenSearchPath = dwRecommendedSearchPath;
                    var action = recommendation;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnCacheAcquireResolving(packageOrContainerId, payloadId, searchPaths, cSearchPaths, fFoundLocal, fVital, dwRecommendedSearchPath, wzDownloadUrl, wzPayloadContainerId, recommendation, ref dwChosenSearchPath, ref action, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)dwChosenSearchPath);
                    w.WriteUInt32((uint)action);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEACQUIRECOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageOrContainerId = a.ReadString()!;
                    var payloadId = a.ReadString()!;
                    var hrStatus = a.ReadInt32();
                    var recommendation = (BOOTSTRAPPER_CACHEACQUIRECOMPLETE_ACTION)a.ReadUInt32();
                    var pAction = recommendation;

                    int hr = CallDispatch(instance, () => instance.OnCacheAcquireComplete(packageOrContainerId, payloadId, hrStatus, recommendation, ref pAction));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)pAction);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEVERIFYBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageOrContainerId = a.ReadString()!;
                    var payloadId = a.ReadString()!;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnCacheVerifyBegin(packageOrContainerId, payloadId, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEVERIFYPROGRESS:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageOrContainerId = a.ReadString()!;
                    var payloadId = a.ReadString()!;
                    var dw64Progress = (long)a.ReadUInt64();
                    var dw64Total = (long)a.ReadUInt64();
                    var dwOverallPercentage = a.ReadInt32();
                    var verifyStep = (CacheVerifyStep)a.ReadUInt32();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnCacheVerifyProgress(packageOrContainerId, payloadId, dw64Progress, dw64Total, dwOverallPercentage, verifyStep, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEVERIFYCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageOrContainerId = a.ReadString()!;
                    var payloadId = a.ReadString()!;
                    var hrStatus = a.ReadInt32();
                    var recommendation = (BOOTSTRAPPER_CACHEVERIFYCOMPLETE_ACTION)a.ReadUInt32();
                    var action = recommendation;

                    int hr = CallDispatch(instance, () => instance.OnCacheVerifyComplete(packageOrContainerId, payloadId, hrStatus, recommendation, ref action));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)action);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEPACKAGECOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var hrStatus = a.ReadInt32();
                    var recommendation = (BOOTSTRAPPER_CACHEPACKAGECOMPLETE_ACTION)a.ReadUInt32();
                    var action = recommendation;

                    int hr = CallDispatch(instance, () => instance.OnCachePackageComplete(packageId, hrStatus, recommendation, ref action));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)action);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHECOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var hrStatus = a.ReadInt32();

                    int hr = CallDispatch(instance, () => instance.OnCacheComplete(hrStatus));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHECONTAINERORPAYLOADVERIFYBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var payloadId = a.ReadString()!;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnCacheContainerOrPayloadVerifyBegin(packageId, payloadId, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHECONTAINERORPAYLOADVERIFYPROGRESS:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageOrContainerId = a.ReadString()!;
                    var payloadId = a.ReadString()!;
                    var dw64Progress = (long)a.ReadUInt64();
                    var dw64Total = (long)a.ReadUInt64();
                    var dwOverallPercentage = a.ReadInt32();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnCacheContainerOrPayloadVerifyProgress(packageOrContainerId, payloadId, dw64Progress, dw64Total, dwOverallPercentage, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHECONTAINERORPAYLOADVERIFYCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var payloadId = a.ReadString()!;
                    var hrStatus = a.ReadInt32();

                    int hr = CallDispatch(instance, () => instance.OnCacheContainerOrPayloadVerifyComplete(packageId, payloadId, hrStatus));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEPAYLOADEXTRACTBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var payloadId = a.ReadString()!;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnCachePayloadExtractBegin(packageId, payloadId, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEPAYLOADEXTRACTPROGRESS:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageOrContainerId = a.ReadString()!;
                    var payloadId = a.ReadString()!;
                    var dw64Progress = (long)a.ReadUInt64();
                    var dw64Total = (long)a.ReadUInt64();
                    var dwOverallPercentage = a.ReadInt32();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnCachePayloadExtractProgress(packageOrContainerId, payloadId, dw64Progress, dw64Total, dwOverallPercentage, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEPAYLOADEXTRACTCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var payloadId = a.ReadString()!;
                    var hrStatus = a.ReadInt32();

                    int hr = CallDispatch(instance, () => instance.OnCachePayloadExtractComplete(packageId, payloadId, hrStatus));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEPACKAGENONVITALVALIDATIONFAILURE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var hrStatus = a.ReadInt32();
                    var recommendation = (BOOTSTRAPPER_CACHEPACKAGENONVITALVALIDATIONFAILURE_ACTION)a.ReadUInt32();
                    var action = recommendation;

                    int hr = CallDispatch(instance, () => instance.OnCachePackageNonVitalValidationFailure(packageId, hrStatus, recommendation, ref action));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)action);
                    return (hr, w.ToArray());
                }

                // -----------------------------------------------------------------------
                // Execute phase
                // -----------------------------------------------------------------------

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var cExecutingPackages = a.ReadInt32();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnExecuteBegin(cExecutingPackages, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEPACKAGEBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var fExecute = a.ReadBool();
                    var action = (ActionState)a.ReadUInt32();
                    var uiLevel = (INSTALLUILEVEL)a.ReadUInt32();
                    var fDisableExternalUiHandler = a.ReadBool();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnExecutePackageBegin(packageId, fExecute, action, uiLevel, fDisableExternalUiHandler, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEPATCHTARGET:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var targetProductCode = a.ReadString()!;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnExecutePatchTarget(packageId, targetProductCode, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEPROGRESS:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var dwProgressPercentage = a.ReadInt32();
                    var dwOverallPercentage = a.ReadInt32();
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnExecuteProgress(packageId, dwProgressPercentage, dwOverallPercentage, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEMSIMESSAGE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var messageType = (InstallMessage)a.ReadUInt32();
                    var dwUIHint = a.ReadInt32();
                    var wzMessage = a.ReadString()!;
                    var cData = (int)a.ReadUInt32();
                    var rgwzData = new string[cData];
                    for (int i = 0; i < cData; i++) { rgwzData[i] = a.ReadString()!; }
                    var nRecommendation = (Result)a.ReadInt32();
                    var pResult = nRecommendation;

                    int hr = CallDispatch(instance, () => instance.OnExecuteMsiMessage(packageId, messageType, dwUIHint, wzMessage, cData, rgwzData, nRecommendation, ref pResult));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteInt32((int)pResult);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEFILESINUSE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var cFiles = (int)a.ReadUInt32();
                    var rgwzFiles = new string[cFiles];
                    for (int i = 0; i < cFiles; i++) { rgwzFiles[i] = a.ReadString()!; }
                    var nRecommendation = (Result)a.ReadInt32();
                    var source = (FilesInUseType)a.ReadUInt32();
                    var pResult = nRecommendation;

                    int hr = CallDispatch(instance, () => instance.OnExecuteFilesInUse(packageId, cFiles, rgwzFiles, nRecommendation, source, ref pResult));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteInt32((int)pResult);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEMBEDDEDCUSTOMMESSAGE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var dwCode = a.ReadInt32();
                    var wzMessage = a.ReadString()!;
                    var pResult = Result.None;

                    int hr = CallDispatch(instance, () => instance.OnEmbeddedCustomMessage(packageId, dwCode, wzMessage, ref pResult));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteInt32((int)pResult);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEPACKAGECOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var hrStatus = a.ReadInt32();
                    var restart = (ApplyRestart)a.ReadUInt32();
                    var recommendation = (BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION)a.ReadUInt32();
                    var pAction = recommendation;

                    int hr = CallDispatch(instance, () => instance.OnExecutePackageComplete(packageId, hrStatus, restart, recommendation, ref pAction));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)pAction);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTECOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var hrStatus = a.ReadInt32();

                    int hr = CallDispatch(instance, () => instance.OnExecuteComplete(hrStatus));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEPROCESSCANCEL:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var packageId = a.ReadString()!;
                    var processId = a.ReadInt32();
                    var recommendation = (BOOTSTRAPPER_EXECUTEPROCESSCANCEL_ACTION)a.ReadUInt32();
                    var pAction = recommendation;

                    int hr = CallDispatch(instance, () => instance.OnExecuteProcessCancel(packageId, processId, recommendation, ref pAction));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)pAction);
                    return (hr, w.ToArray());
                }

                // -----------------------------------------------------------------------
                // Post-execute
                // -----------------------------------------------------------------------

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONUNREGISTERBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var recommendedRegistrationType = (RegistrationType)a.ReadUInt32();
                    var pRegistrationType = recommendedRegistrationType;

                    int hr = CallDispatch(instance, () => instance.OnUnregisterBegin(recommendedRegistrationType, ref pRegistrationType));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)pRegistrationType);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONUNREGISTERCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var hrStatus = a.ReadInt32();

                    int hr = CallDispatch(instance, () => instance.OnUnregisterComplete(hrStatus));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONAPPLYCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var hrStatus = a.ReadInt32();
                    var restart = (ApplyRestart)a.ReadUInt32();
                    var recommendation = (BOOTSTRAPPER_APPLYCOMPLETE_ACTION)a.ReadUInt32();
                    var pAction = recommendation;

                    int hr = CallDispatch(instance, () => instance.OnApplyComplete(hrStatus, restart, recommendation, ref pAction));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)pAction);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONAPPLYDOWNGRADE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var hrRecommended = a.ReadInt32();
                    var hrStatus = hrRecommended;

                    int hr = CallDispatch(instance, () => instance.OnApplyDowngrade(hrRecommended, ref hrStatus));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteInt32(hrStatus);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONLAUNCHAPPROVEDEXEBEGIN:
                {
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnLaunchApprovedExeBegin(ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONLAUNCHAPPROVEDEXECOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var hrStatus = a.ReadInt32();
                    var processId = a.ReadInt32();

                    int hr = CallDispatch(instance, () => instance.OnLaunchApprovedExeComplete(hrStatus, processId));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                // -----------------------------------------------------------------------
                // MSI Transactions
                // -----------------------------------------------------------------------

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONBEGINMSITRANSACTIONBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var transactionId = a.ReadString()!;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnBeginMsiTransactionBegin(transactionId, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONBEGINMSITRANSACTIONCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var transactionId = a.ReadString()!;
                    var hrStatus = a.ReadInt32();
                    var restart = (ApplyRestart)a.ReadUInt32();
                    var recommendation = (BOOTSTRAPPER_BEGINMSITRANSACTIONCOMPLETE_ACTION)a.ReadUInt32();
                    var pAction = recommendation;

                    int hr = CallDispatch(instance, () => instance.OnBeginMsiTransactionComplete(transactionId, hrStatus, restart, recommendation, ref pAction));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)pAction);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCOMMITMSITRANSACTIONBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var transactionId = a.ReadString()!;
                    bool fCancel = false;

                    int hr = CallDispatch(instance, () => instance.OnCommitMsiTransactionBegin(transactionId, ref fCancel));
                    if (instance.TestShouldCancel) { fCancel = true; }
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteBool(fCancel);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCOMMITMSITRANSACTIONCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var transactionId = a.ReadString()!;
                    var hrStatus = a.ReadInt32();
                    var restart = (ApplyRestart)a.ReadUInt32();
                    var recommendation = (BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION)a.ReadUInt32();
                    var pAction = recommendation;

                    int hr = CallDispatch(instance, () => instance.OnCommitMsiTransactionComplete(transactionId, hrStatus, restart, recommendation, ref pAction));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)pAction);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONROLLBACKMSITRANSACTIONBEGIN:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var transactionId = a.ReadString()!;

                    int hr = CallDispatch(instance, () => instance.OnRollbackMsiTransactionBegin(transactionId));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONROLLBACKMSITRANSACTIONCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var transactionId = a.ReadString()!;
                    var hrStatus = a.ReadInt32();
                    var restart = (ApplyRestart)a.ReadUInt32();
                    var recommendation = (BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION)a.ReadUInt32();
                    var pAction = recommendation;

                    int hr = CallDispatch(instance, () => instance.OnRollbackMsiTransactionComplete(transactionId, hrStatus, restart, recommendation, ref pAction));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    w.WriteUInt32((uint)pAction);
                    return (hr, w.ToArray());
                }

                // -----------------------------------------------------------------------
                // Windows Update / System Restore
                // -----------------------------------------------------------------------

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPAUSEAUTOMATICUPDATESBEGIN:
                {
                    int hr = CallDispatch(instance, () => instance.OnPauseAutomaticUpdatesBegin());
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONPAUSEAUTOMATICUPDATESCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var hrStatus = a.ReadInt32();

                    int hr = CallDispatch(instance, () => instance.OnPauseAutomaticUpdatesComplete(hrStatus));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONSYSTEMRESTOREPOINTBEGIN:
                {
                    int hr = CallDispatch(instance, () => instance.OnSystemRestorePointBegin());
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONSYSTEMRESTOREPOINTCOMPLETE:
                {
                    var a = ctx.GetArgsReader();
                    a.ReadUInt32(); // apiVersion
                    var hrStatus = a.ReadInt32();

                    int hr = CallDispatch(instance, () => instance.OnSystemRestorePointComplete(hrStatus));
                    if (ctx.WasForwarded) { return (ctx.ResponseHr, ctx.ResponseData); }
                    var w = new BurnBufferWriter();
                    w.WriteUInt32(BurnProtocolConstants.ApiVersion);
                    return (hr, w.ToArray());
                }

                default:
                    // Unknown message: forward if possible, otherwise return E_NOTIMPL.
                    if (ctx.RealBA != null)
                    {
                        ctx.ForwardToRealBA();
                        return (ctx.ResponseHr, ctx.ResponseData);
                    }
                    return (unchecked((int)0x80004001u) /* E_NOTIMPL */, Array.Empty<byte>());
            }
        }
    }
}
