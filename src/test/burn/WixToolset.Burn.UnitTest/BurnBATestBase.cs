// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Burn.UnitTest
{
    using System;
    using WixToolset.BootstrapperApplicationApi;
    using WixToolset.Burn.UnitTest.Internal;

    /// <summary>
    /// Base class for burn bootstrapper application unit tests.
    /// Implements <see cref="IBootstrapperApplication"/> so that all BA lifecycle callbacks
    /// are virtual methods that closely resemble what a native BA implementation would look like.
    ///
    /// The default implementation of every method forwards the message to the real BA and returns
    /// the real BA's response. Override a method to observe arguments, run assertions, or take
    /// control of the response.  To forward after inspecting call <c>base.OnXxx(...)</c>.
    ///
    /// A class that overrides nothing is a valid test: it simply verifies that the bundle completes
    /// without any unhandled exception.
    /// </summary>
    public class BurnBATestBase : IBootstrapperApplication
    {
        /// <summary>
        /// Gets the per-iteration inline data provided via <see cref="BurnBAInlineDataAttribute"/>.
        /// Empty array when no attribute was applied.
        /// </summary>
        public object[] TestData { get; internal set; } = Array.Empty<object>();

        /// <summary>
        /// Gets the 0-based iteration index within the owning test class (inline data index).
        /// </summary>
        public int TestIteration { get; internal set; }

        /// <summary>
        /// Per-dispatch context.  Set by the runner/dispatcher before each virtual method call.
        /// </summary>
        internal BurnBAMessageContext _messageContext;

        // -----------------------------------------------------------------------
        // Low-level COM fallbacks – not intended for test overrides.
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public int BAProc(int message, IntPtr pvArgs, IntPtr pvResults) => 0;

        /// <inheritdoc/>
        public void BAProcFallback(int message, IntPtr pvArgs, IntPtr pvResults, ref int phr) { }

        // -----------------------------------------------------------------------
        // Lifecycle
        // -----------------------------------------------------------------------

        /// <summary>
        /// Called when the engine creates the BA.  The <paramref name="engine"/> parameter is null
        /// in test mode; use the real BA to perform engine interactions.
        /// The default implementation forwards to the real BA.
        /// </summary>
        public virtual int OnCreate(IBootstrapperEngine engine, ref Command command)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <summary>
        /// Called when the engine destroys the BA.
        /// Default implementation forwards to the real BA.
        /// </summary>
        public virtual int OnDestroy(bool reload)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <summary>
        /// Called when the engine starts up.
        /// Default implementation forwards to the real BA.
        /// </summary>
        public virtual int OnStartup()
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <summary>
        /// Called when the engine is about to shut down.
        /// Default implementation forwards to the real BA and reads back the shutdown action.
        /// </summary>
        public virtual int OnShutdown(ref BOOTSTRAPPER_SHUTDOWN_ACTION action)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            action = (BOOTSTRAPPER_SHUTDOWN_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        // -----------------------------------------------------------------------
        // Detect phase
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public virtual int OnDetectBegin(bool fCached, RegistrationType registrationType, int cPackages, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectForwardCompatibleBundle(string wzBundleCode, RelationType relationType, string wzBundleTag, bool fPerMachine, string wzVersion, bool fMissingFromCache, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectUpdateBegin(string wzUpdateLocation, ref bool fCancel, ref bool fSkip)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            fSkip = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectUpdate(string wzUpdateLocation, long dw64Size, string wzHash, UpdateHashType hashAlgorithm, string wzVersion, string wzTitle, string wzSummary, string wzContentType, string wzContent, ref bool fCancel, ref bool fStopProcessingUpdates)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            fStopProcessingUpdates = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectUpdateComplete(int hrStatus, ref bool fIgnoreError)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fIgnoreError = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectRelatedBundle(string wzBundleCode, RelationType relationType, string wzBundleTag, bool fPerMachine, string wzVersion, bool fMissingFromCache, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectPackageBegin(string wzPackageId, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectCompatibleMsiPackage(string wzPackageId, string wzCompatiblePackageId, string wzCompatiblePackageVersion, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectRelatedMsiPackage(string wzPackageId, string wzUpgradeCode, string wzProductCode, bool fPerMachine, string wzVersion, RelatedOperation operation, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectPatchTarget(string wzPackageId, string wzProductCode, PackageState patchState, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectMsiFeature(string wzPackageId, string wzFeatureId, FeatureState state, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectPackageComplete(string wzPackageId, int hrStatus, PackageState state, bool fCached)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectComplete(int hrStatus, bool fEligibleForCleanup)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectRelatedBundlePackage(string wzPackageId, string wzBundleCode, RelationType relationType, bool fPerMachine, string wzVersion, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        // -----------------------------------------------------------------------
        // Plan phase
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public virtual int OnPlanBegin(int cPackages, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanRelatedBundle(string wzBundleCode, RequestState recommendedState, ref RequestState pRequestedState, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            pRequestedState = (RequestState)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanMsiTransaction(string wzTransactionId, ref bool fTransaction, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fTransaction = r.ReadBool();
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanMsiTransactionComplete(string wzTransactionId, uint dwPackagesInTransaction, bool fPlanned, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanPackageBegin(string wzPackageId, PackageState state, bool fCached, BOOTSTRAPPER_PACKAGE_CONDITION_RESULT installCondition, BOOTSTRAPPER_PACKAGE_CONDITION_RESULT repairCondition, RequestState recommendedState, BOOTSTRAPPER_CACHE_TYPE recommendedCacheType, ref RequestState pRequestedState, ref BOOTSTRAPPER_CACHE_TYPE pRequestedCacheType, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            pRequestedState = (RequestState)r.ReadUInt32();
            pRequestedCacheType = (BOOTSTRAPPER_CACHE_TYPE)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanCompatibleMsiPackageBegin(string wzPackageId, string wzCompatiblePackageId, string wzCompatiblePackageVersion, bool fRecommendedRemove, ref bool fRequestRemove, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            fRequestRemove = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanCompatibleMsiPackageComplete(string wzPackageId, string wzCompatiblePackageId, int hrStatus, bool fRequestedRemove)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanPatchTarget(string wzPackageId, string wzProductCode, RequestState recommendedState, ref RequestState pRequestedState, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            pRequestedState = (RequestState)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanMsiFeature(string wzPackageId, string wzFeatureId, FeatureState recommendedState, ref FeatureState pRequestedState, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pRequestedState = (FeatureState)r.ReadUInt32();
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanMsiPackage(string wzPackageId, bool fExecute, ActionState action, BOOTSTRAPPER_MSI_FILE_VERSIONING recommendedFileVersioning, ref bool fCancel, ref BURN_MSI_PROPERTY actionMsiProperty, ref INSTALLUILEVEL uiLevel, ref bool fDisableExternalUiHandler, ref BOOTSTRAPPER_MSI_FILE_VERSIONING fileVersioning)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            actionMsiProperty = (BURN_MSI_PROPERTY)r.ReadUInt32();
            uiLevel = (INSTALLUILEVEL)r.ReadUInt32();
            fDisableExternalUiHandler = r.ReadBool();
            fileVersioning = (BOOTSTRAPPER_MSI_FILE_VERSIONING)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanPackageComplete(string wzPackageId, int hrStatus, RequestState requested)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlannedCompatiblePackage(string wzPackageId, string wzCompatiblePackageId, bool fRemove)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlannedPackage(string wzPackageId, ActionState execute, ActionState rollback, bool fPlannedCache, bool fPlannedUncache)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanForwardCompatibleBundle(string wzBundleCode, RelationType relationType, string wzBundleTag, bool fPerMachine, string wzVersion, bool fRecommendedIgnoreBundle, ref bool fCancel, ref bool fIgnoreBundle)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            fIgnoreBundle = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanRestoreRelatedBundle(string wzBundleCode, RequestState recommendedState, ref RequestState pRequestedState, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            pRequestedState = (RequestState)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanRelatedBundleType(string wzBundleCode, RelatedBundlePlanType recommendedType, ref RelatedBundlePlanType pRequestedType, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            pRequestedType = (RelatedBundlePlanType)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        // -----------------------------------------------------------------------
        // Apply phase
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public virtual int OnApplyBegin(int dwPhaseCount, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnElevateBegin(ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnElevateComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnProgress(int dwProgressPercentage, int dwOverallPercentage, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnError(ErrorType errorType, string wzPackageId, int dwCode, string wzError, int dwUIHint, int cData, string[] rgwzData, Result nRecommendation, ref Result pResult)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pResult = (Result)r.ReadInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnRegisterBegin(RegistrationType recommendedRegistrationType, ref bool fCancel, ref RegistrationType pRegistrationType)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            pRegistrationType = (RegistrationType)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnRegisterComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        // -----------------------------------------------------------------------
        // Cache phase
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public virtual int OnCacheBegin(ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCachePackageBegin(string wzPackageId, int cCachePayloads, long dw64PackageCacheSize, bool fVital, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheAcquireBegin(string wzPackageOrContainerId, string wzPayloadId, string wzSource, string wzDownloadUrl, string wzPayloadContainerId, CacheOperation recommendation, ref CacheOperation action, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            action = (CacheOperation)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheAcquireProgress(string wzPackageOrContainerId, string wzPayloadId, long dw64Progress, long dw64Total, int dwOverallPercentage, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheAcquireResolving(string wzPackageOrContainerId, string wzPayloadId, string[] searchPaths, int cSearchPaths, bool fFoundLocal, bool fVital, int dwRecommendedSearchPath, string wzDownloadUrl, string wzPayloadContainerId, CacheResolveOperation recommendation, ref int dwChosenSearchPath, ref CacheResolveOperation action, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            dwChosenSearchPath = (int)r.ReadUInt32();
            action = (CacheResolveOperation)r.ReadUInt32();
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheAcquireComplete(string wzPackageOrContainerId, string wzPayloadId, int hrStatus, BOOTSTRAPPER_CACHEACQUIRECOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_CACHEACQUIRECOMPLETE_ACTION pAction)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pAction = (BOOTSTRAPPER_CACHEACQUIRECOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheVerifyBegin(string wzPackageOrContainerId, string wzPayloadId, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheVerifyProgress(string wzPackageOrContainerId, string wzPayloadId, long dw64Progress, long dw64Total, int dwOverallPercentage, CacheVerifyStep verifyStep, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheVerifyComplete(string wzPackageOrContainerId, string wzPayloadId, int hrStatus, BOOTSTRAPPER_CACHEVERIFYCOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_CACHEVERIFYCOMPLETE_ACTION action)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            action = (BOOTSTRAPPER_CACHEVERIFYCOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCachePackageComplete(string wzPackageId, int hrStatus, BOOTSTRAPPER_CACHEPACKAGECOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_CACHEPACKAGECOMPLETE_ACTION action)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            action = (BOOTSTRAPPER_CACHEPACKAGECOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheContainerOrPayloadVerifyBegin(string wzPackageId, string wzPayloadId, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheContainerOrPayloadVerifyProgress(string wzPackageOrContainerId, string wzPayloadId, long dw64Progress, long dw64Total, int dwOverallPercentage, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheContainerOrPayloadVerifyComplete(string wzPackageId, string wzPayloadId, int hrStatus)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCachePayloadExtractBegin(string wzPackageId, string wzPayloadId, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCachePayloadExtractProgress(string wzPackageOrContainerId, string wzPayloadId, long dw64Progress, long dw64Total, int dwOverallPercentage, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCachePayloadExtractComplete(string wzPackageId, string wzPayloadId, int hrStatus)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCachePackageNonVitalValidationFailure(string wzPackageId, int hrStatus, BOOTSTRAPPER_CACHEPACKAGENONVITALVALIDATIONFAILURE_ACTION recommendation, ref BOOTSTRAPPER_CACHEPACKAGENONVITALVALIDATIONFAILURE_ACTION action)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            action = (BOOTSTRAPPER_CACHEPACKAGENONVITALVALIDATIONFAILURE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        // -----------------------------------------------------------------------
        // Execute phase
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public virtual int OnExecuteBegin(int cExecutingPackages, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecutePackageBegin(string wzPackageId, bool fExecute, ActionState action, INSTALLUILEVEL uiLevel, bool fDisableExternalUiHandler, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecutePatchTarget(string wzPackageId, string wzTargetProductCode, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecuteProgress(string wzPackageId, int dwProgressPercentage, int dwOverallPercentage, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecuteMsiMessage(string wzPackageId, InstallMessage messageType, int dwUIHint, string wzMessage, int cData, string[] rgwzData, Result nRecommendation, ref Result pResult)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pResult = (Result)r.ReadInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecuteFilesInUse(string wzPackageId, int cFiles, string[] rgwzFiles, Result nRecommendation, FilesInUseType source, ref Result pResult)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pResult = (Result)r.ReadInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnEmbeddedCustomMessage(string wzPackageId, int dwCode, string wzMessage, ref Result pResult)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pResult = (Result)r.ReadInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecutePackageComplete(string wzPackageId, int hrStatus, ApplyRestart restart, BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION pAction)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pAction = (BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecuteComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecuteProcessCancel(string wzPackageId, int processId, BOOTSTRAPPER_EXECUTEPROCESSCANCEL_ACTION recommendation, ref BOOTSTRAPPER_EXECUTEPROCESSCANCEL_ACTION pAction)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pAction = (BOOTSTRAPPER_EXECUTEPROCESSCANCEL_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        // -----------------------------------------------------------------------
        // Post-execute / wrap-up
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public virtual int OnUnregisterBegin(RegistrationType recommendedRegistrationType, ref RegistrationType pRegistrationType)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pRegistrationType = (RegistrationType)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnUnregisterComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnApplyComplete(int hrStatus, ApplyRestart restart, BOOTSTRAPPER_APPLYCOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_APPLYCOMPLETE_ACTION pAction)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pAction = (BOOTSTRAPPER_APPLYCOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnApplyDowngrade(int hrRecommended, ref int hrStatus)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            hrStatus = r.ReadInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnLaunchApprovedExeBegin(ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnLaunchApprovedExeComplete(int hrStatus, int processId)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        // -----------------------------------------------------------------------
        // MSI transactions
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public virtual int OnBeginMsiTransactionBegin(string wzTransactionId, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnBeginMsiTransactionComplete(string wzTransactionId, int hrStatus, ApplyRestart restart, BOOTSTRAPPER_BEGINMSITRANSACTIONCOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_BEGINMSITRANSACTIONCOMPLETE_ACTION pAction)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pAction = (BOOTSTRAPPER_BEGINMSITRANSACTIONCOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCommitMsiTransactionBegin(string wzTransactionId, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCommitMsiTransactionComplete(string wzTransactionId, int hrStatus, ApplyRestart restart, BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION pAction)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pAction = (BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnRollbackMsiTransactionBegin(string wzTransactionId)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnRollbackMsiTransactionComplete(string wzTransactionId, int hrStatus, ApplyRestart restart, BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION pAction)
        {
            _messageContext!.ForwardToRealBA();
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pAction = (BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        // -----------------------------------------------------------------------
        // Windows Update / System Restore
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public virtual int OnPauseAutomaticUpdatesBegin()
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPauseAutomaticUpdatesComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnSystemRestorePointBegin()
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnSystemRestorePointComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA();
            return _messageContext.ResponseHr;
        }
    }
}
