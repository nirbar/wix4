// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest.Mimic
{
    using System;
    using System.Collections.Generic;
    using WixToolset.BootstrapperApplicationApi;
    using WixToolset.Burn.UnitTest.Internal;

    /// <summary>
    /// Fires synthetic apply-phase callbacks directly on a <see cref="BurnBATestBase"/> instance
    /// when mimic-engine mode is active.  No packages are installed or uninstalled.
    /// <para>
    /// The sequence mirrors what burn sends during a real apply:
    /// OnApplyBegin → OnRegisterBegin/Complete → OnCacheBegin/Complete →
    /// OnExecuteBegin → (per package) OnExecutePackageBegin / OnExecuteMsiMessage* /
    /// OnExecuteProgress / OnExecutePackageComplete → OnExecuteComplete →
    /// OnUnregisterBegin/Complete → OnApplyComplete.
    /// </para>
    /// </summary>
    internal static class MimicApplySimulator
    {
        /// <summary>
        /// Runs the simulated apply phase synchronously on the pump thread.
        /// Called after the runner detects <see cref="BurnBATestBase._mimicApplyPending"/>.
        /// </summary>
        /// <param name="instance">The test instance.</param>
        /// <param name="plannedPackages">
        /// Ordered list of (packageId, executeAction) pairs recorded from
        /// <c>ONPLANNEDPACKAGE</c> messages during the real Plan phase.
        /// Packages with <see cref="ActionState.None"/> are skipped.
        /// </param>
        internal static void Simulate(
            BurnBATestBase instance,
            IReadOnlyList<(string PackageId, ActionState ExecuteAction)> plannedPackages)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (plannedPackages == null) throw new ArgumentNullException(nameof(plannedPackages));

            int overallHr = 0;
            bool earlyCancel = false;

            try
            {
                // ---- ApplyBegin ----
                instance._applyBeginSeen = true;
                bool fCancel = false;
                SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONAPPLYBEGIN);
                instance.OnApplyBegin(1 /* phaseCount */, ref fCancel);
                if (CheckCancel(instance, fCancel)) { earlyCancel = true; goto applyComplete; }

                // ---- Register ----
                var regType = RegistrationType.None;
                SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONREGISTERBEGIN);
                instance.OnRegisterBegin(RegistrationType.None, ref fCancel, ref regType);
                if (CheckCancel(instance, fCancel)) { earlyCancel = true; goto applyComplete; }
                SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONREGISTERCOMPLETE);
                instance.OnRegisterComplete(0);
                if (CheckCancel(instance, false)) { earlyCancel = true; goto applyComplete; }

                // ---- Cache (no-op: no payloads to download) ----
                SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHEBEGIN);
                instance.OnCacheBegin(ref fCancel);
                if (CheckCancel(instance, fCancel)) { earlyCancel = true; goto applyComplete; }
                SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONCACHECOMPLETE);
                instance.OnCacheComplete(0);
                if (CheckCancel(instance, false)) { earlyCancel = true; goto applyComplete; }

                // ---- Execute ----
                int executingCount = CountExecuting(plannedPackages);
                SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEBEGIN);
                instance.OnExecuteBegin(executingCount, ref fCancel);
                if (CheckCancel(instance, fCancel)) { earlyCancel = true; goto applyComplete; }

                foreach (var (pkgId, execAction) in plannedPackages)
                {
                    if (execAction == ActionState.None) continue;
                    if (CheckCancel(instance, false)) { earlyCancel = true; break; }

                    // OnExecutePackageBegin
                    SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEPACKAGEBEGIN);
                    instance.OnExecutePackageBegin(pkgId, fExecute: true, execAction,
                        INSTALLUILEVEL.None, fDisableExternalUiHandler: false, ref fCancel);
                    if (CheckCancel(instance, fCancel)) { earlyCancel = true; break; }

                    // OnExecuteMsiMessage — configured per package by the test
                    if (instance.MimicMsiMessages.TryGetValue(pkgId, out var msgs) && msgs != null)
                    {
                        foreach (var msg in msgs)
                        {
                            if (CheckCancel(instance, false)) break;
                            var nResult = Result.None;
                            SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEMSIMESSAGE);
                            instance.OnExecuteMsiMessage(pkgId, msg.MessageType, msg.UiFlags,
                                msg.Message, 0, Array.Empty<string>(), Result.None, ref nResult);
                        }
                    }
                    if (CheckCancel(instance, false)) { earlyCancel = true; break; }

                    // OnExecuteProgress — report 100% for simplicity
                    SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEPROGRESS);
                    instance.OnExecuteProgress(pkgId, 100, 100, ref fCancel);
                    if (CheckCancel(instance, fCancel)) { earlyCancel = true; break; }

                    // Per-package HRESULT (default S_OK)
                    instance.MimicApplyResults.TryGetValue(pkgId, out int pkgHr);
                    if (overallHr == 0 && pkgHr != 0) { overallHr = pkgHr; }

                    // OnExecutePackageComplete
                    var execCompleteAction = BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION.None;
                    SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTEPACKAGECOMPLETE);
                    instance.OnExecutePackageComplete(pkgId, pkgHr, ApplyRestart.None,
                        BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION.None, ref execCompleteAction);
                }

                if (!CheckCancel(instance, false))
                {
                    SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONEXECUTECOMPLETE);
                    instance.OnExecuteComplete(overallHr);
                }

                // ---- Unregister ----
                if (!CheckCancel(instance, false))
                {
                    var unregType = RegistrationType.None;
                    SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONUNREGISTERBEGIN);
                    instance.OnUnregisterBegin(RegistrationType.None, ref unregType);
                    SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONUNREGISTERCOMPLETE);
                    instance.OnUnregisterComplete(0);
                }
            }
            catch (Exception ex)
            {
                instance.AddException(ex, endAutoPilot: false);
                if (overallHr == 0) { overallHr = unchecked((int)0x80004005); } // E_FAIL
            }

            applyComplete:
            // OnApplyComplete always fires, even when cancelled or on error.
            try
            {
                instance._applyCompleteSeen = true;
                var applyAction = BOOTSTRAPPER_APPLYCOMPLETE_ACTION.None;
                SetCtx(instance, BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONAPPLYCOMPLETE);
                instance.OnApplyComplete(
                    earlyCancel ? unchecked((int)0x800704C7) /* E_CANCELLED */ : overallHr,
                    ApplyRestart.None,
                    BOOTSTRAPPER_APPLYCOMPLETE_ACTION.None,
                    ref applyAction);
            }
            catch (Exception ex)
            {
                instance.AddException(ex, endAutoPilot: false);
            }
            finally
            {
                instance._messageContext = null;
            }
        }

        private static void SetCtx(BurnBATestBase instance, BurnApplicationMessage msg)
        {
            instance._messageContext = BurnBAMessageContext.CreateForMimic((uint)msg);
        }

        private static bool CheckCancel(BurnBATestBase instance, bool fCancel)
            => fCancel || instance.TestShouldCancel;

        private static int CountExecuting(IReadOnlyList<(string PackageId, ActionState ExecuteAction)> packages)
        {
            int count = 0;
            foreach (var (_, action) in packages)
            {
                if (action != ActionState.None) count++;
            }
            return count;
        }
    }
}
