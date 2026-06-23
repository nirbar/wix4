namespace WixToolset.Burn.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.ExceptionServices;
    using WixToolset.BootstrapperApplicationApi;
    using WixToolset.Burn.UnitTest.Internal;
    using WixToolset.Burn.UnitTest.Mimic;

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
    public class BurnBATestBase : IBootstrapperApplication, IDisposable
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
        /// Gets the parsed command passed to <see cref="OnCreate"/>.
        /// Available inside any BA callback after <see cref="OnCreate"/> has been called.
        /// </summary>
        public TestBaCommand Command { get; internal set; }

        /// <summary>
        /// Gets the engine interface that forwards calls to the burn engine through the
        /// .BAEngine named pipe.  Available inside any BA callback after
        /// <see cref="OnCreate"/> has been called.
        /// Use this instead of the <see cref="IBootstrapperEngine"/> parameter of
        /// <see cref="OnCreate"/>, which is always <see langword="null"/> in test mode.
        /// In mimic mode this is replaced with a <c>MimicApplyEngine</c> wrapper after
        /// <see cref="OnCreate"/> is dispatched.
        /// </summary>
        public IEngine Engine { get; internal set; }

        /// <summary>
        /// When set to <see langword="true"/> the dispatcher skips all further test-BA and
        /// real-BA callbacks and drives burn to a clean shutdown without test involvement.
        ///
        /// Before the apply phase: detect and plan complete with default (non-cancel) values.
        /// At <c>OnDetectComplete</c> and <c>OnPlanComplete</c> the dispatcher sets a flag that
        /// causes the runner to call <c>Engine.Quit()</c> so burn shuts down instead of
        /// waiting indefinitely for the BA to call <c>Plan()</c> or <c>Apply()</c>.
        ///
        /// During the apply phase (from <c>OnApplyBegin</c> up to but not including
        /// <c>OnApplyComplete</c>): every cancellable message gets <c>fCancel = true</c> so
        /// burn stops executing packages and eventually sends <c>OnApplyComplete</c>, after
        /// which burn calls <c>OnShutdown</c> on its own.
        ///
        /// In all cases, if a real BA is connected it receives a synthetic <c>OnShutdown</c>
        /// before <c>Engine.Quit()</c> is sent so it terminates cleanly.
        /// </summary>
        public bool EndTestAutoPilot { get; set; }

        /// <summary>
        /// Add and exception and, when <paramref name="endAutoPilot"/>
        /// is <see langword="true"/> (the default), also sets
        /// <see cref="EndTestAutoPilot"/> to drive burn to a clean shutdown automatically.
        /// </summary>
        public void AddException(Exception ex, bool endAutoPilot = true)
        {
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }
            this.Exceptions.Add(ex);
            if (endAutoPilot)
            {
                this.EndTestAutoPilot = true;
            }
        }

        /// <summary>
        /// Unhandled exceptions store
        /// </summary>
        public List<Exception> Exceptions { get; set; } = new List<Exception>();

        // -----------------------------------------------------------------------
        // Mimic-engine mode
        // -----------------------------------------------------------------------

        /// <summary>
        /// Per-package HRESULT overrides for the simulated apply phase.
        /// Key = package id, Value = HRESULT returned in
        /// <see cref="OnExecutePackageComplete"/> for that package.
        /// Any package not present in the dictionary defaults to <c>S_OK (0)</c>.
        /// Only meaningful when <see cref="BurnBATestClassAttribute.MimicEngine"/> is
        /// <see langword="true"/>.
        /// </summary>
        public Dictionary<string, int> MimicApplyResults { get; } = new Dictionary<string, int>();

        /// <summary>
        /// Per-package MSI messages to fire during the simulated execute phase, in order.
        /// Key = package id; Value = ordered list of messages sent via
        /// <see cref="OnExecuteMsiMessage"/> after <see cref="OnExecutePackageBegin"/>.
        /// Only meaningful when <see cref="BurnBATestClassAttribute.MimicEngine"/> is
        /// <see langword="true"/>.
        /// </summary>
        public Dictionary<string, List<MimicMsiMessage>> MimicMsiMessages { get; } = new Dictionary<string, List<MimicMsiMessage>>();

        /// <summary>
        /// Set to <see langword="true"/> by <c>MimicApplyEngine.Apply()</c> to signal the
        /// runner that the apply phase should be simulated rather than sent to burn.
        /// </summary>
        internal bool _mimicApplyPending;

        /// <summary>
        /// <see langword="true"/> when the test is running in mimic-engine mode.
        /// Set by the runner before <see cref="Initialize"/> is called.
        /// </summary>
        internal bool _isMimicMode;

        /// <summary>
        /// Whether or not any unhandled exceptions occured.
        /// </summary>
        public bool HasExceptions => this.Exceptions.Count > 0;

        /// <summary>
        /// <see langword="true"/> when the dispatcher should set <c>fCancel = true</c> on any
        /// cancellable message.  This combines the failure-cancel and the autopilot cancel so
        /// both checks in the dispatcher can be expressed as a single property read.
        /// Autopilot cancels only messages during the apply phase (after <c>OnApplyBegin</c>
        /// and before <c>OnApplyComplete</c>); messages outside that window are not cancelled.
        /// </summary>
        internal bool TestShouldCancel
            => this.HasExceptions
            || (this.EndTestAutoPilot && this._applyBeginSeen && !this._applyCompleteSeen);

        // ---- Phase-tracking fields (written by the dispatcher) ----

        /// <summary>Set to <see langword="true"/> the moment <c>OnApplyBegin</c> is received.</summary>
        internal bool _applyBeginSeen;

        /// <summary>Set to <see langword="true"/> the moment <c>OnApplyComplete</c> is received.</summary>
        internal bool _applyCompleteSeen;

        /// <summary>
        /// Set by the dispatcher when <c>OnDetectComplete</c> or <c>OnPlanComplete</c> is
        /// received while autopilot is active.  The runner checks this after writing the BA
        /// response and calls <c>Engine.Quit()</c> to avoid live-locking the burn engine.
        /// </summary>
        internal bool _pendingEngineQuit;

        /// <summary>
        /// Per-dispatch context.  Set by the runner/dispatcher before each virtual method call.
        /// </summary>
        internal BurnBAMessageContext _messageContext;

        internal void Initialize(IEngine engine)
        {
            this.Engine = engine;
            try
            {
                if (engine.ContainsVariable("WixBundleLog"))
                {
                    var log = engine.GetVariableString("WixBundleLog");
                    if (!string.IsNullOrEmpty(log))
                    {
                        this._logFolder = Path.GetDirectoryName(log);
                        this._logPattern = Path.GetFileNameWithoutExtension(log) + "*.*";
                    }
                }
            }
            catch { }
        }

        private string _logFolder;
        private string _logPattern;
        public IEnumerable<string> GetLogFiles()
        {
            List<string> logFiles = new List<string>();
            try
            {
                foreach (var log in Directory.GetFiles(this._logFolder, this._logPattern))
                {
                    logFiles.Add(log);
                }
            }
            catch { }
            return logFiles;
        }

        /// <summary>
        /// Finalize the test result. Override to ignore specific exceptions or impose any other
        /// test result functionality.
        /// Overriders are encouraged to filter the <see cref="Exceptions"/> list and then call
        /// <see cref="BurnBATestBase.FinalizeResult()"/> as doing so allows the bundle log files
        /// to be attached to the test report.
        /// By default, throws the first exception, or if no exception has been recorded, do nothing.
        /// </summary>
        public virtual void FinalizeResult()
        {
            var ex = this.Exceptions.FirstOrDefault();
            if (ex != null)
            {
                ExceptionDispatchInfo.Capture(ex).Throw();
            }
        }

        /// <summary>
        /// Called by the test framework when an exception is thrown by any BA callback override,
        /// and again at the end of each test iteration.
        /// Override to release resources or reset state that should not survive a test failure.
        /// Implementations should be safe to call more than once.
        /// The default implementation is a no-op.
        /// </summary>
        public virtual void Dispose() { }

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
            this.Engine.Log(LogLevel.Standard, $"Starting unit test '{this.GetType().Name}'");
            _messageContext!.ForwardOnCreateToRealBA(this, new TestBaCommand(command));
            return _messageContext.ResponseHr;
        }

        /// <summary>
        /// Called when the engine destroys the BA.
        /// Default implementation forwards to the real BA.
        /// </summary>
        public virtual int OnDestroy(bool reload)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <summary>
        /// Called when the engine starts up.
        /// Default implementation forwards to the real BA.
        /// </summary>
        public virtual int OnStartup()
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <summary>
        /// Called when the engine is about to shut down.
        /// Default implementation forwards to the real BA and reads back the shutdown action.
        /// </summary>
        public virtual int OnShutdown(ref BOOTSTRAPPER_SHUTDOWN_ACTION action)
        {
            _messageContext!.ForwardToRealBA(this);
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
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectForwardCompatibleBundle(string wzBundleCode, RelationType relationType, string wzBundleTag, bool fPerMachine, string wzVersion, bool fMissingFromCache, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectUpdateBegin(string wzUpdateLocation, ref bool fCancel, ref bool fSkip)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            fSkip = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectUpdate(string wzUpdateLocation, long dw64Size, string wzHash, UpdateHashType hashAlgorithm, string wzVersion, string wzTitle, string wzSummary, string wzContentType, string wzContent, ref bool fCancel, ref bool fStopProcessingUpdates)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            fStopProcessingUpdates = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectUpdateComplete(int hrStatus, ref bool fIgnoreError)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fIgnoreError = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectRelatedBundle(string wzBundleCode, RelationType relationType, string wzBundleTag, bool fPerMachine, string wzVersion, bool fMissingFromCache, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectPackageBegin(string wzPackageId, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectCompatibleMsiPackage(string wzPackageId, string wzCompatiblePackageId, string wzCompatiblePackageVersion, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectRelatedMsiPackage(string wzPackageId, string wzUpgradeCode, string wzProductCode, bool fPerMachine, string wzVersion, RelatedOperation operation, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectPatchTarget(string wzPackageId, string wzProductCode, PackageState patchState, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectMsiFeature(string wzPackageId, string wzFeatureId, FeatureState state, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectPackageComplete(string wzPackageId, int hrStatus, PackageState state, bool fCached)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectComplete(int hrStatus, bool fEligibleForCleanup)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnDetectRelatedBundlePackage(string wzPackageId, string wzBundleCode, RelationType relationType, bool fPerMachine, string wzVersion, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
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
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanRelatedBundle(string wzBundleCode, RequestState recommendedState, ref RequestState pRequestedState, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            pRequestedState = (RequestState)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanMsiTransaction(string wzTransactionId, ref bool fTransaction, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fTransaction = r.ReadBool();
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanMsiTransactionComplete(string wzTransactionId, uint dwPackagesInTransaction, bool fPlanned, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanPackageBegin(string wzPackageId, PackageState state, bool fCached, BOOTSTRAPPER_PACKAGE_CONDITION_RESULT installCondition, BOOTSTRAPPER_PACKAGE_CONDITION_RESULT repairCondition, RequestState recommendedState, BOOTSTRAPPER_CACHE_TYPE recommendedCacheType, ref RequestState pRequestedState, ref BOOTSTRAPPER_CACHE_TYPE pRequestedCacheType, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
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
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            fRequestRemove = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanCompatibleMsiPackageComplete(string wzPackageId, string wzCompatiblePackageId, int hrStatus, bool fRequestedRemove)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanPatchTarget(string wzPackageId, string wzProductCode, RequestState recommendedState, ref RequestState pRequestedState, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            pRequestedState = (RequestState)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanMsiFeature(string wzPackageId, string wzFeatureId, FeatureState recommendedState, ref FeatureState pRequestedState, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pRequestedState = (FeatureState)r.ReadUInt32();
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanMsiPackage(string wzPackageId, bool fExecute, ActionState action, BOOTSTRAPPER_MSI_FILE_VERSIONING recommendedFileVersioning, ref bool fCancel, ref BURN_MSI_PROPERTY actionMsiProperty, ref INSTALLUILEVEL uiLevel, ref bool fDisableExternalUiHandler, ref BOOTSTRAPPER_MSI_FILE_VERSIONING fileVersioning)
        {
            _messageContext!.ForwardToRealBA(this);
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
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlannedCompatiblePackage(string wzPackageId, string wzCompatiblePackageId, bool fRemove)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlannedPackage(string wzPackageId, ActionState execute, ActionState rollback, bool fPlannedCache, bool fPlannedUncache)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanForwardCompatibleBundle(string wzBundleCode, RelationType relationType, string wzBundleTag, bool fPerMachine, string wzVersion, bool fRecommendedIgnoreBundle, ref bool fCancel, ref bool fIgnoreBundle)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            fIgnoreBundle = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanRestoreRelatedBundle(string wzBundleCode, RequestState recommendedState, ref RequestState pRequestedState, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            pRequestedState = (RequestState)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPlanRelatedBundleType(string wzBundleCode, RelatedBundlePlanType recommendedType, ref RelatedBundlePlanType pRequestedType, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
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
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnElevateBegin(ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnElevateComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnProgress(int dwProgressPercentage, int dwOverallPercentage, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnError(ErrorType errorType, string wzPackageId, int dwCode, string wzError, int dwUIHint, int cData, string[] rgwzData, Result nRecommendation, ref Result pResult)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pResult = (Result)r.ReadInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnRegisterBegin(RegistrationType recommendedRegistrationType, ref bool fCancel, ref RegistrationType pRegistrationType)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            pRegistrationType = (RegistrationType)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnRegisterComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        // -----------------------------------------------------------------------
        // Cache phase
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public virtual int OnCacheBegin(ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCachePackageBegin(string wzPackageId, int cCachePayloads, long dw64PackageCacheSize, bool fVital, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheAcquireBegin(string wzPackageOrContainerId, string wzPayloadId, string wzSource, string wzDownloadUrl, string wzPayloadContainerId, CacheOperation recommendation, ref CacheOperation action, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            action = (CacheOperation)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheAcquireProgress(string wzPackageOrContainerId, string wzPayloadId, long dw64Progress, long dw64Total, int dwOverallPercentage, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheAcquireResolving(string wzPackageOrContainerId, string wzPayloadId, string[] searchPaths, int cSearchPaths, bool fFoundLocal, bool fVital, int dwRecommendedSearchPath, string wzDownloadUrl, string wzPayloadContainerId, CacheResolveOperation recommendation, ref int dwChosenSearchPath, ref CacheResolveOperation action, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
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
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pAction = (BOOTSTRAPPER_CACHEACQUIRECOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheVerifyBegin(string wzPackageOrContainerId, string wzPayloadId, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheVerifyProgress(string wzPackageOrContainerId, string wzPayloadId, long dw64Progress, long dw64Total, int dwOverallPercentage, CacheVerifyStep verifyStep, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheVerifyComplete(string wzPackageOrContainerId, string wzPayloadId, int hrStatus, BOOTSTRAPPER_CACHEVERIFYCOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_CACHEVERIFYCOMPLETE_ACTION action)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            action = (BOOTSTRAPPER_CACHEVERIFYCOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCachePackageComplete(string wzPackageId, int hrStatus, BOOTSTRAPPER_CACHEPACKAGECOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_CACHEPACKAGECOMPLETE_ACTION action)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            action = (BOOTSTRAPPER_CACHEPACKAGECOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheContainerOrPayloadVerifyBegin(string wzPackageId, string wzPayloadId, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheContainerOrPayloadVerifyProgress(string wzPackageOrContainerId, string wzPayloadId, long dw64Progress, long dw64Total, int dwOverallPercentage, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCacheContainerOrPayloadVerifyComplete(string wzPackageId, string wzPayloadId, int hrStatus)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCachePayloadExtractBegin(string wzPackageId, string wzPayloadId, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCachePayloadExtractProgress(string wzPackageOrContainerId, string wzPayloadId, long dw64Progress, long dw64Total, int dwOverallPercentage, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCachePayloadExtractComplete(string wzPackageId, string wzPayloadId, int hrStatus)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCachePackageNonVitalValidationFailure(string wzPackageId, int hrStatus, BOOTSTRAPPER_CACHEPACKAGENONVITALVALIDATIONFAILURE_ACTION recommendation, ref BOOTSTRAPPER_CACHEPACKAGENONVITALVALIDATIONFAILURE_ACTION action)
        {
            _messageContext!.ForwardToRealBA(this);
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
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecutePackageBegin(string wzPackageId, bool fExecute, ActionState action, INSTALLUILEVEL uiLevel, bool fDisableExternalUiHandler, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecutePatchTarget(string wzPackageId, string wzTargetProductCode, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecuteProgress(string wzPackageId, int dwProgressPercentage, int dwOverallPercentage, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecuteMsiMessage(string wzPackageId, InstallMessage messageType, int dwUIHint, string wzMessage, int cData, string[] rgwzData, Result nRecommendation, ref Result pResult)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pResult = (Result)r.ReadInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecuteFilesInUse(string wzPackageId, int cFiles, string[] rgwzFiles, Result nRecommendation, FilesInUseType source, ref Result pResult)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pResult = (Result)r.ReadInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnEmbeddedCustomMessage(string wzPackageId, int dwCode, string wzMessage, ref Result pResult)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pResult = (Result)r.ReadInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecutePackageComplete(string wzPackageId, int hrStatus, ApplyRestart restart, BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION pAction)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pAction = (BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecuteComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnExecuteProcessCancel(string wzPackageId, int processId, BOOTSTRAPPER_EXECUTEPROCESSCANCEL_ACTION recommendation, ref BOOTSTRAPPER_EXECUTEPROCESSCANCEL_ACTION pAction)
        {
            _messageContext!.ForwardToRealBA(this);
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
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pRegistrationType = (RegistrationType)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnUnregisterComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnApplyComplete(int hrStatus, ApplyRestart restart, BOOTSTRAPPER_APPLYCOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_APPLYCOMPLETE_ACTION pAction)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pAction = (BOOTSTRAPPER_APPLYCOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnApplyDowngrade(int hrRecommended, ref int hrStatus)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            hrStatus = r.ReadInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnLaunchApprovedExeBegin(ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnLaunchApprovedExeComplete(int hrStatus, int processId)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        // -----------------------------------------------------------------------
        // MSI transactions
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public virtual int OnBeginMsiTransactionBegin(string wzTransactionId, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnBeginMsiTransactionComplete(string wzTransactionId, int hrStatus, ApplyRestart restart, BOOTSTRAPPER_BEGINMSITRANSACTIONCOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_BEGINMSITRANSACTIONCOMPLETE_ACTION pAction)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pAction = (BOOTSTRAPPER_BEGINMSITRANSACTIONCOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCommitMsiTransactionBegin(string wzTransactionId, ref bool fCancel)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            fCancel = r.ReadBool();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnCommitMsiTransactionComplete(string wzTransactionId, int hrStatus, ApplyRestart restart, BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION pAction)
        {
            _messageContext!.ForwardToRealBA(this);
            var r = new BurnBufferReader(_messageContext.ResponseData);
            r.ReadUInt32(); // apiVersion
            pAction = (BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION)r.ReadUInt32();
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnRollbackMsiTransactionBegin(string wzTransactionId)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnRollbackMsiTransactionComplete(string wzTransactionId, int hrStatus, ApplyRestart restart, BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION recommendation, ref BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION pAction)
        {
            _messageContext!.ForwardToRealBA(this);
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
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnPauseAutomaticUpdatesComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnSystemRestorePointBegin()
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <inheritdoc/>
        public virtual int OnSystemRestorePointComplete(int hrStatus)
        {
            _messageContext!.ForwardToRealBA(this);
            return _messageContext.ResponseHr;
        }

        /// <summary>
        /// Called by the unit test framework to request graceful shutdown of the bootstrapper application.
        /// The engine pipe has already been disconnected; do NOT call engine methods.
        /// The default implementation is a no-op (the test host process has no UI event loop).
        /// </summary>
        public virtual void OnUnitTestShutdown()
        {
        }
    }
}
