#pragma once
// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.


enum BOOTSTRAPPER_DISPLAY
{
    BOOTSTRAPPER_DISPLAY_UNKNOWN,
    BOOTSTRAPPER_DISPLAY_EMBEDDED,
    BOOTSTRAPPER_DISPLAY_NONE,
    BOOTSTRAPPER_DISPLAY_PASSIVE,
    BOOTSTRAPPER_DISPLAY_FULL,
};

enum BOOTSTRAPPER_REGISTRATION_TYPE
{
    BOOTSTRAPPER_REGISTRATION_TYPE_NONE, // The engine will ignore NONE if it recommended INPROGRESS or FULL.
    BOOTSTRAPPER_REGISTRATION_TYPE_INPROGRESS,
    BOOTSTRAPPER_REGISTRATION_TYPE_FULL,
};

enum BOOTSTRAPPER_RESUME_TYPE
{
    BOOTSTRAPPER_RESUME_TYPE_NONE,
    BOOTSTRAPPER_RESUME_TYPE_INVALID,        // resume information is present but invalid
    BOOTSTRAPPER_RESUME_TYPE_INTERRUPTED,    // relaunched after an unexpected interruption
    BOOTSTRAPPER_RESUME_TYPE_REBOOT,         // relaunched after reboot
    BOOTSTRAPPER_RESUME_TYPE_SUSPEND,        // relaunched after suspend
    BOOTSTRAPPER_RESUME_TYPE_ARP,            // launched from ARP
};

enum BOOTSTRAPPER_ERROR_TYPE
{
    BOOTSTRAPPER_ERROR_TYPE_ELEVATE,            // error occurred trying to elevate.
    BOOTSTRAPPER_ERROR_TYPE_WINDOWS_INSTALLER,  // error came from windows installer.
    BOOTSTRAPPER_ERROR_TYPE_EXE_PACKAGE,        // error came from an exe package.
    BOOTSTRAPPER_ERROR_TYPE_HTTP_AUTH_SERVER,   // error occurred trying to authenticate with HTTP server.
    BOOTSTRAPPER_ERROR_TYPE_HTTP_AUTH_PROXY,    // error occurred trying to authenticate with HTTP proxy.
    BOOTSTRAPPER_ERROR_TYPE_APPLY,              // error occurred during apply.
};

enum BOOTSTRAPPER_FILES_IN_USE_TYPE
{
    BOOTSTRAPPER_FILES_IN_USE_TYPE_MSI,    // INSTALLMESSAGE_FILESINUSE
    BOOTSTRAPPER_FILES_IN_USE_TYPE_MSI_RM, // INSTALLMESSAGE_RMFILESINUSE
    BOOTSTRAPPER_FILES_IN_USE_TYPE_NETFX,  // MMIO_CLOSE_APPS
};

enum BOOTSTRAPPER_RELATED_OPERATION
{
    BOOTSTRAPPER_RELATED_OPERATION_NONE,
    BOOTSTRAPPER_RELATED_OPERATION_DOWNGRADE,
    BOOTSTRAPPER_RELATED_OPERATION_MINOR_UPDATE,
    BOOTSTRAPPER_RELATED_OPERATION_MAJOR_UPGRADE,
    BOOTSTRAPPER_RELATED_OPERATION_REMOVE,
    BOOTSTRAPPER_RELATED_OPERATION_INSTALL,
    BOOTSTRAPPER_RELATED_OPERATION_REPAIR,
};

enum BOOTSTRAPPER_CACHE_OPERATION
{
    // There is no source available.
    BOOTSTRAPPER_CACHE_OPERATION_NONE,
    // Copy the payload or container from the chosen local source.
    BOOTSTRAPPER_CACHE_OPERATION_COPY,
    // Download the payload or container using the download URL.
    BOOTSTRAPPER_CACHE_OPERATION_DOWNLOAD,
    // Extract the payload from the container.
    BOOTSTRAPPER_CACHE_OPERATION_EXTRACT,
};

enum BOOTSTRAPPER_CACHE_RESOLVE_OPERATION
{
    // There is no source available.
    BOOTSTRAPPER_CACHE_RESOLVE_NONE,
    // Copy the payload or container from the chosen local source.
    BOOTSTRAPPER_CACHE_RESOLVE_LOCAL,
    // Download the payload or container from the download URL.
    BOOTSTRAPPER_CACHE_RESOLVE_DOWNLOAD,
    // Extract the payload from the container.
    BOOTSTRAPPER_CACHE_RESOLVE_CONTAINER,
    // Look again for the payload or container locally.
    BOOTSTRAPPER_CACHE_RESOLVE_RETRY,
};

enum BOOTSTRAPPER_CACHE_VERIFY_STEP
{
    BOOTSTRAPPER_CACHE_VERIFY_STEP_STAGE,
    BOOTSTRAPPER_CACHE_VERIFY_STEP_HASH,
    BOOTSTRAPPER_CACHE_VERIFY_STEP_FINALIZE,
};

enum BOOTSTRAPPER_APPLY_RESTART
{
    BOOTSTRAPPER_APPLY_RESTART_NONE,
    BOOTSTRAPPER_APPLY_RESTART_REQUIRED,
    BOOTSTRAPPER_APPLY_RESTART_INITIATED,
};

enum BOOTSTRAPPER_RELATION_TYPE
{
    BOOTSTRAPPER_RELATION_NONE,
    BOOTSTRAPPER_RELATION_DETECT,
    BOOTSTRAPPER_RELATION_UPGRADE,
    BOOTSTRAPPER_RELATION_ADDON,
    BOOTSTRAPPER_RELATION_PATCH,
    BOOTSTRAPPER_RELATION_DEPENDENT_ADDON,
    BOOTSTRAPPER_RELATION_DEPENDENT_PATCH,
    BOOTSTRAPPER_RELATION_UPDATE,
    BOOTSTRAPPER_RELATION_CHAIN_PACKAGE,
};

enum BOOTSTRAPPER_RELATED_BUNDLE_PLAN_TYPE
{
    BOOTSTRAPPER_RELATED_BUNDLE_PLAN_TYPE_NONE,
    BOOTSTRAPPER_RELATED_BUNDLE_PLAN_TYPE_DOWNGRADE,
    BOOTSTRAPPER_RELATED_BUNDLE_PLAN_TYPE_UPGRADE,
    BOOTSTRAPPER_RELATED_BUNDLE_PLAN_TYPE_ADDON,
    BOOTSTRAPPER_RELATED_BUNDLE_PLAN_TYPE_PATCH,
    BOOTSTRAPPER_RELATED_BUNDLE_PLAN_TYPE_DEPENDENT_ADDON,
    BOOTSTRAPPER_RELATED_BUNDLE_PLAN_TYPE_DEPENDENT_PATCH,
};

enum BOOTSTRAPPER_CACHE_TYPE
{
    BOOTSTRAPPER_CACHE_TYPE_REMOVE,
    BOOTSTRAPPER_CACHE_TYPE_KEEP,
    BOOTSTRAPPER_CACHE_TYPE_FORCE,
};

enum BOOTSTRAPPER_PACKAGE_CONDITION_RESULT
{
    BOOTSTRAPPER_PACKAGE_CONDITION_DEFAULT,
    BOOTSTRAPPER_PACKAGE_CONDITION_FALSE,
    BOOTSTRAPPER_PACKAGE_CONDITION_TRUE,
};

enum BOOTSTRAPPER_MSI_FILE_VERSIONING
{
    BOOTSTRAPPER_MSI_FILE_VERSIONING_MISSING_OR_OLDER,          //o
    BOOTSTRAPPER_MSI_FILE_VERSIONING_MISSING_OR_OLDER_OR_EQUAL, //e
    BOOTSTRAPPER_MSI_FILE_VERSIONING_ALL,                       //a
};

enum BOOTSTRAPPER_APPLICATION_MESSAGE
{
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTBEGIN,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTCOMPLETE,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANBEGIN,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANCOMPLETE,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONSTARTUP,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONSHUTDOWN,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTFORWARDCOMPATIBLEBUNDLE,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTUPDATEBEGIN,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTUPDATE,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTUPDATECOMPLETE,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTRELATEDBUNDLE,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTPACKAGEBEGIN,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTRELATEDMSIPACKAGE,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTPATCHTARGET,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTMSIFEATURE,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONDETECTPACKAGECOMPLETE,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANRELATEDBUNDLE,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANPACKAGEBEGIN,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANPATCHTARGET,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANMSIFEATURE,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONPLANPACKAGECOMPLETE,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONAPPLYBEGIN,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONELEVATEBEGIN,
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
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONSETUPDATEBEGIN,
    BOOTSTRAPPER_APPLICATION_MESSAGE_ONSETUPDATECOMPLETE,
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
};

enum BOOTSTRAPPER_APPLYCOMPLETE_ACTION
{
    BOOTSTRAPPER_APPLYCOMPLETE_ACTION_NONE,
    // Instructs the engine to restart.
    // The engine will not launch again after the machine is rebooted.
    // Ignored if reboot was already initiated by OnExecutePackageComplete().
    BOOTSTRAPPER_APPLYCOMPLETE_ACTION_RESTART,
};

enum BOOTSTRAPPER_CACHEACQUIRECOMPLETE_ACTION
{
    BOOTSTRAPPER_CACHEACQUIRECOMPLETE_ACTION_NONE,
    // Instructs the engine to try the acquisition of the payload again.
    // Ignored if hrStatus is a success.
    BOOTSTRAPPER_CACHEACQUIRECOMPLETE_ACTION_RETRY,
};

enum BOOTSTRAPPER_CACHEPACKAGECOMPLETE_ACTION
{
    BOOTSTRAPPER_CACHEPACKAGECOMPLETE_ACTION_NONE,
    // Instructs the engine to ignore non-vital package failures and
    // continue with the caching.
    // Ignored if hrStatus is a success or the package is vital.
    BOOTSTRAPPER_CACHEPACKAGECOMPLETE_ACTION_IGNORE,
    // Instructs the engine to try the acquisition and verification of the package again.
    // Ignored if hrStatus is a success.
    BOOTSTRAPPER_CACHEPACKAGECOMPLETE_ACTION_RETRY,
};

enum BOOTSTRAPPER_CACHEPACKAGENONVITALVALIDATIONFAILURE_ACTION
{
    BOOTSTRAPPER_CACHEPACKAGENONVITALVALIDATIONFAILURE_ACTION_NONE,
    // Instructs the engine to try to acquire the package so execution can use it.
    // Most of the time this is used for installing the package during rollback.
    BOOTSTRAPPER_CACHEPACKAGENONVITALVALIDATIONFAILURE_ACTION_ACQUIRE,
};

enum BOOTSTRAPPER_CACHEVERIFYCOMPLETE_ACTION
{
    BOOTSTRAPPER_CACHEVERIFYCOMPLETE_ACTION_NONE,
    // Ignored if hrStatus is a success.
    BOOTSTRAPPER_CACHEVERIFYCOMPLETE_ACTION_RETRYVERIFICATION,
    // Ignored if hrStatus is a success.
    BOOTSTRAPPER_CACHEVERIFYCOMPLETE_ACTION_RETRYACQUISITION,
};

enum BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION
{
    BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION_NONE,
    // Instructs the engine to ignore non-vital package failures and
    // continue with the install.
    // Ignored if hrStatus is a success or the package is vital.
    BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION_IGNORE,
    // Instructs the engine to try the execution of the package again.
    // Ignored if hrStatus is a success.
    BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION_RETRY,
    // Instructs the engine to stop processing the chain and restart.
    // The engine will launch again after the machine is restarted.
    BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION_RESTART,
    // Instructs the engine to stop processing the chain and
    // suspend the current state.
    BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION_SUSPEND,
};

enum BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION
{
    BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION_NONE,
    // Instructs the engine to stop processing the chain and restart.
    // The engine will launch again after the machine is restarted.
    BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION_RESTART,
};

enum BOOTSTRAPPER_EXECUTEPROCESSCANCEL_ACTION
{
    // Instructs the engine to stop waiting for the process to exit.
    // The package is immediately considered to have failed with ERROR_INSTALL_USEREXIT.
    // The engine will never rollback the package.
    BOOTSTRAPPER_EXECUTEPROCESSCANCEL_ACTION_ABANDON,
    // Instructs the engine to wait for the process to exit.
    // Once the process has exited, the package is considered to have failed with ERROR_INSTALL_USEREXIT.
    // This allows the engine to rollback the package if necessary.
    BOOTSTRAPPER_EXECUTEPROCESSCANCEL_ACTION_WAIT,
};

enum BOOTSTRAPPER_SHUTDOWN_ACTION
{
    BOOTSTRAPPER_SHUTDOWN_ACTION_NONE,
    // Instructs the engine to restart.
    // The engine will not launch again after the machine is rebooted.
    // Ignored if reboot was already initiated by OnExecutePackageComplete().
    BOOTSTRAPPER_SHUTDOWN_ACTION_RESTART,
    // Instructs the engine to unload the bootstrapper application and
    // restart the engine which will load the bootstrapper application again.
    // Typically used to switch from a native bootstrapper application to a managed one.
    BOOTSTRAPPER_SHUTDOWN_ACTION_RELOAD_BOOTSTRAPPER,
    // Opts out of the engine behavior of trying to uninstall itself
    // when no non-permanent packages are installed.
    BOOTSTRAPPER_SHUTDOWN_ACTION_SKIP_CLEANUP,
};

enum BURN_MSI_PROPERTY
{
    BURN_MSI_PROPERTY_NONE,     // no property added
    BURN_MSI_PROPERTY_INSTALL,  // add BURNMSIINSTALL=1
    BURN_MSI_PROPERTY_MODIFY,   // add BURNMSIMODIFY=1
    BURN_MSI_PROPERTY_REPAIR,   // add BURNMSIREPAIR=1
    BURN_MSI_PROPERTY_UNINSTALL,// add BURNMSIUNINSTALL=1
};

struct BOOTSTRAPPER_COMMAND
{
    DWORD cbSize;
    BOOTSTRAPPER_ACTION action;
    BOOTSTRAPPER_DISPLAY display;

    LPWSTR wzCommandLine;
    int nCmdShow;

    BOOTSTRAPPER_RESUME_TYPE resumeType;
    HWND hwndSplashScreen;

    // If this was run from a related bundle, specifies the relation type
    BOOTSTRAPPER_RELATION_TYPE relationType;
    BOOL fPassthrough;

    LPWSTR wzLayoutDirectory;
    LPWSTR wzBootstrapperWorkingFolder;
    LPWSTR wzBootstrapperApplicationDataPath;
};

struct BA_ONAPPLYBEGIN_ARGS
{
    DWORD cbSize;
    DWORD dwPhaseCount;
};

struct BA_ONAPPLYBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONAPPLYCOMPLETE_ARGS
{
    DWORD cbSize;
    HRESULT hrStatus;
    // Indicates whether any package required a reboot or initiated the reboot already.
    BOOTSTRAPPER_APPLY_RESTART restart;
    BOOTSTRAPPER_APPLYCOMPLETE_ACTION recommendation;
};

struct BA_ONAPPLYCOMPLETE_RESULTS
{
    DWORD cbSize;
    BOOTSTRAPPER_APPLYCOMPLETE_ACTION action;
};

struct BA_ONAPPLYDOWNGRADE_ARGS
{
    DWORD cbSize;
    HRESULT hrRecommended;
};

struct BA_ONAPPLYDOWNGRADE_RESULTS
{
    DWORD cbSize;
    HRESULT hrStatus;
};

struct BA_ONBEGINMSITRANSACTIONBEGIN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzTransactionId;
};

struct BA_ONBEGINMSITRANSACTIONBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONBEGINMSITRANSACTIONCOMPLETE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzTransactionId;
    HRESULT hrStatus;
};

struct BA_ONBEGINMSITRANSACTIONCOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONCACHEACQUIREBEGIN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageOrContainerId;
    LPCWSTR wzPayloadId;
    LPCWSTR wzSource;
    LPCWSTR wzDownloadUrl;
    LPCWSTR wzPayloadContainerId;
    BOOTSTRAPPER_CACHE_OPERATION recommendation;
};

struct BA_ONCACHEACQUIREBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
    BOOTSTRAPPER_CACHE_OPERATION action;
};

struct BA_ONCACHEACQUIRECOMPLETE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageOrContainerId;
    LPCWSTR wzPayloadId;
    HRESULT hrStatus;
    BOOTSTRAPPER_CACHEACQUIRECOMPLETE_ACTION recommendation;
};

struct BA_ONCACHEACQUIRECOMPLETE_RESULTS
{
    DWORD cbSize;
    BOOTSTRAPPER_CACHEACQUIRECOMPLETE_ACTION action;
};

struct BA_ONCACHEACQUIREPROGRESS_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageOrContainerId;
    LPCWSTR wzPayloadId;
    DWORD64 dw64Progress;
    DWORD64 dw64Total;
    DWORD dwOverallPercentage;
};

struct BA_ONCACHEACQUIREPROGRESS_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONCACHEACQUIRERESOLVING_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageOrContainerId;
    LPCWSTR wzPayloadId;
    LPCWSTR* rgSearchPaths;
    DWORD cSearchPaths;
    BOOL fFoundLocal;
    DWORD dwRecommendedSearchPath;
    LPCWSTR wzDownloadUrl;
    LPCWSTR wzPayloadContainerId;
    BOOTSTRAPPER_CACHE_RESOLVE_OPERATION recommendation;
};

struct BA_ONCACHEACQUIRERESOLVING_RESULTS
{
    DWORD cbSize;
    DWORD dwChosenSearchPath;
    BOOTSTRAPPER_CACHE_RESOLVE_OPERATION action;
    BOOL fCancel;
};

struct BA_ONCACHEBEGIN_ARGS
{
    DWORD cbSize;
};

struct BA_ONCACHEBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONCACHECOMPLETE_ARGS
{
    DWORD cbSize;
    HRESULT hrStatus;
};

struct BA_ONCACHECOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONCACHECONTAINERORPAYLOADVERIFYBEGIN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageOrContainerId;
    LPCWSTR wzPayloadId;
};

struct BA_ONCACHECONTAINERORPAYLOADVERIFYBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONCACHECONTAINERORPAYLOADVERIFYCOMPLETE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageOrContainerId;
    LPCWSTR wzPayloadId;
    HRESULT hrStatus;
};

struct BA_ONCACHECONTAINERORPAYLOADVERIFYCOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONCACHECONTAINERORPAYLOADVERIFYPROGRESS_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageOrContainerId;
    LPCWSTR wzPayloadId;
    DWORD64 dw64Progress;
    DWORD64 dw64Total;
    DWORD dwOverallPercentage;
};

struct BA_ONCACHECONTAINERORPAYLOADVERIFYPROGRESS_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONCACHEPACKAGEBEGIN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    DWORD cCachePayloads;
    DWORD64 dw64PackageCacheSize;
    // If caching a package is not vital, then acquisition will be skipped unless the BA opts in through OnCachePackageNonVitalValidationFailure.
    BOOL fVital;
};

struct BA_ONCACHEPACKAGEBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONCACHEPACKAGECOMPLETE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    HRESULT hrStatus;
    BOOTSTRAPPER_CACHEPACKAGECOMPLETE_ACTION recommendation;
};

struct BA_ONCACHEPACKAGECOMPLETE_RESULTS
{
    DWORD cbSize;
    BOOTSTRAPPER_CACHEPACKAGECOMPLETE_ACTION action;
};

struct BA_ONCACHEPACKAGENONVITALVALIDATIONFAILURE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    HRESULT hrStatus;
    BOOTSTRAPPER_CACHEPACKAGENONVITALVALIDATIONFAILURE_ACTION recommendation;
};

struct BA_ONCACHEPACKAGENONVITALVALIDATIONFAILURE_RESULTS
{
    DWORD cbSize;
    BOOTSTRAPPER_CACHEPACKAGENONVITALVALIDATIONFAILURE_ACTION action;
};

struct BA_ONCACHEPAYLOADEXTRACTBEGIN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzContainerId;
    LPCWSTR wzPayloadId;
};

struct BA_ONCACHEPAYLOADEXTRACTBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONCACHEPAYLOADEXTRACTCOMPLETE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzContainerId;
    LPCWSTR wzPayloadId;
    HRESULT hrStatus;
};

struct BA_ONCACHEPAYLOADEXTRACTCOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONCACHEPAYLOADEXTRACTPROGRESS_ARGS
{
    DWORD cbSize;
    LPCWSTR wzContainerId;
    LPCWSTR wzPayloadId;
    DWORD64 dw64Progress;
    DWORD64 dw64Total;
    DWORD dwOverallPercentage;
};

struct BA_ONCACHEPAYLOADEXTRACTPROGRESS_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONCACHEVERIFYBEGIN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageOrContainerId;
    LPCWSTR wzPayloadId;
};

struct BA_ONCACHEVERIFYBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONCACHEVERIFYCOMPLETE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageOrContainerId;
    LPCWSTR wzPayloadId;
    HRESULT hrStatus;
    BOOTSTRAPPER_CACHEVERIFYCOMPLETE_ACTION recommendation;
};

struct BA_ONCACHEVERIFYCOMPLETE_RESULTS
{
    DWORD cbSize;
    BOOTSTRAPPER_CACHEVERIFYCOMPLETE_ACTION action;
};

struct BA_ONCACHEVERIFYPROGRESS_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageOrContainerId;
    LPCWSTR wzPayloadId;
    DWORD64 dw64Progress;
    DWORD64 dw64Total;
    DWORD dwOverallPercentage;
    BOOTSTRAPPER_CACHE_VERIFY_STEP verifyStep;
};

struct BA_ONCACHEVERIFYPROGRESS_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONCOMMITMSITRANSACTIONBEGIN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzTransactionId;
};

struct BA_ONCOMMITMSITRANSACTIONBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONCOMMITMSITRANSACTIONCOMPLETE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzTransactionId;
    HRESULT hrStatus;
    BOOTSTRAPPER_APPLY_RESTART restart;
    BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION recommendation;
};

struct BA_ONCOMMITMSITRANSACTIONCOMPLETE_RESULTS
{
    DWORD cbSize;
    BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION action;
};

struct BA_ONDETECTBEGIN_ARGS
{
    DWORD cbSize;
    BOOTSTRAPPER_REGISTRATION_TYPE registrationType;
    DWORD cPackages;
    BOOL fCached;
};

struct BA_ONDETECTBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONDETECTCOMPATIBLEMSIPACKAGE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    LPCWSTR wzCompatiblePackageId;
    LPCWSTR wzCompatiblePackageVersion;
};

struct BA_ONDETECTCOMPATIBLEMSIPACKAGE_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONDETECTCOMPLETE_ARGS
{
    DWORD cbSize;
    HRESULT hrStatus;
    BOOL fEligibleForCleanup;
};

struct BA_ONDETECTCOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONDETECTFORWARDCOMPATIBLEBUNDLE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzBundleId;
    BOOTSTRAPPER_RELATION_TYPE relationType;
    LPCWSTR wzBundleTag;
    BOOL fPerMachine;
    LPCWSTR wzVersion;
    BOOL fMissingFromCache;
};

struct BA_ONDETECTFORWARDCOMPATIBLEBUNDLE_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONDETECTMSIFEATURE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    LPCWSTR wzFeatureId;
    BOOTSTRAPPER_FEATURE_STATE state;
};

struct BA_ONDETECTMSIFEATURE_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONDETECTPACKAGEBEGIN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
};

struct BA_ONDETECTPACKAGEBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONDETECTPACKAGECOMPLETE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    HRESULT hrStatus;
    BOOTSTRAPPER_PACKAGE_STATE state;
    BOOL fCached;
};

struct BA_ONDETECTPACKAGECOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONDETECTRELATEDBUNDLE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzBundleId;
    BOOTSTRAPPER_RELATION_TYPE relationType;
    LPCWSTR wzBundleTag;
    BOOL fPerMachine;
    LPCWSTR wzVersion;
    BOOL fMissingFromCache;
};

struct BA_ONDETECTRELATEDBUNDLE_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONDETECTRELATEDBUNDLEPACKAGE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    LPCWSTR wzBundleId;
    BOOTSTRAPPER_RELATION_TYPE relationType;
    BOOL fPerMachine;
    LPCWSTR wzVersion;
};

struct BA_ONDETECTRELATEDBUNDLEPACKAGE_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONDETECTRELATEDMSIPACKAGE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    LPCWSTR wzUpgradeCode;
    LPCWSTR wzProductCode;
    BOOL fPerMachine;
    LPCWSTR wzVersion;
    BOOTSTRAPPER_RELATED_OPERATION operation;
};

struct BA_ONDETECTRELATEDMSIPACKAGE_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONDETECTPATCHTARGET_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    LPCWSTR wzProductCode;
    BOOTSTRAPPER_PACKAGE_STATE patchState;
};

struct BA_ONDETECTPATCHTARGET_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONDETECTUPDATE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzUpdateLocation;
    DWORD64 dw64Size;
    LPCWSTR wzHash;
    BOOTSTRAPPER_UPDATE_HASH_TYPE hashAlgorithm;
    LPCWSTR wzVersion;
    LPCWSTR wzTitle;
    LPCWSTR wzSummary;
    LPCWSTR wzContentType;
    LPCWSTR wzContent;
};

struct BA_ONDETECTUPDATE_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
    BOOL fStopProcessingUpdates;
};

struct BA_ONDETECTUPDATEBEGIN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzUpdateLocation;
};

struct BA_ONDETECTUPDATEBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
    BOOL fSkip;
};

struct BA_ONDETECTUPDATECOMPLETE_ARGS
{
    DWORD cbSize;
    HRESULT hrStatus;
};

struct BA_ONDETECTUPDATECOMPLETE_RESULTS
{
    DWORD cbSize;
    BOOL fIgnoreError;
};

struct BA_ONELEVATEBEGIN_ARGS
{
    DWORD cbSize;
};

struct BA_ONELEVATEBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONELEVATECOMPLETE_ARGS
{
    DWORD cbSize;
    HRESULT hrStatus;
};

struct BA_ONELEVATECOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONERROR_ARGS
{
    DWORD cbSize;
    BOOTSTRAPPER_ERROR_TYPE errorType;
    LPCWSTR wzPackageId;
    DWORD dwCode;
    LPCWSTR wzError;
    DWORD dwUIHint;
    DWORD cData;
    LPCWSTR* rgwzData;
    int nRecommendation;
};

struct BA_ONERROR_RESULTS
{
    DWORD cbSize;
    int nResult;
};

struct BA_ONEXECUTEBEGIN_ARGS
{
    DWORD cbSize;
    DWORD cExecutingPackages;
};

struct BA_ONEXECUTEBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONEXECUTECOMPLETE_ARGS
{
    DWORD cbSize;
    HRESULT hrStatus;
};

struct BA_ONEXECUTECOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONEXECUTEFILESINUSE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    DWORD cFiles;
    LPCWSTR* rgwzFiles;
    int nRecommendation;
    BOOTSTRAPPER_FILES_IN_USE_TYPE source;
};

struct BA_ONEXECUTEFILESINUSE_RESULTS
{
    DWORD cbSize;
    int nResult;
};

struct BA_ONEXECUTEMSIMESSAGE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    INSTALLMESSAGE messageType;
    DWORD dwUIHint;
    LPCWSTR wzMessage;
    DWORD cData;
    LPCWSTR* rgwzData;
    int nRecommendation;
};

struct BA_ONEXECUTEMSIMESSAGE_RESULTS
{
    DWORD cbSize;
    int nResult;
};

struct BA_ONEXECUTEPACKAGEBEGIN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    BOOL fExecute; // false means rollback.
    BOOTSTRAPPER_ACTION_STATE action;
    INSTALLUILEVEL uiLevel;
    BOOL fDisableExternalUiHandler;
};

struct BA_ONEXECUTEPACKAGEBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONEXECUTEPACKAGECOMPLETE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    HRESULT hrStatus;
    // Indicates whether this package requires a reboot or initiated the reboot already.
    BOOTSTRAPPER_APPLY_RESTART restart;
    BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION recommendation;
};

struct BA_ONEXECUTEPACKAGECOMPLETE_RESULTS
{
    DWORD cbSize;
    BOOTSTRAPPER_EXECUTEPACKAGECOMPLETE_ACTION action;
};

struct BA_ONEXECUTEPATCHTARGET_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    LPCWSTR wzTargetProductCode;
};

struct BA_ONEXECUTEPATCHTARGET_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONEXECUTEPROCESSCANCEL_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    DWORD dwProcessId;
    BOOTSTRAPPER_EXECUTEPROCESSCANCEL_ACTION recommendation;
};

struct BA_ONEXECUTEPROCESSCANCEL_RESULTS
{
    DWORD cbSize;
    BOOTSTRAPPER_EXECUTEPROCESSCANCEL_ACTION action;
};

struct BA_ONEXECUTEPROGRESS_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    DWORD dwProgressPercentage;
    DWORD dwOverallPercentage;
};

struct BA_ONEXECUTEPROGRESS_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONLAUNCHAPPROVEDEXEBEGIN_ARGS
{
    DWORD cbSize;
};

struct BA_ONLAUNCHAPPROVEDEXEBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONLAUNCHAPPROVEDEXECOMPLETE_ARGS
{
    DWORD cbSize;
    HRESULT hrStatus;
    // Only valid if the operation succeeded.
    DWORD dwProcessId;
};

struct BA_ONLAUNCHAPPROVEDEXECOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONPAUSEAUTOMATICUPDATESBEGIN_ARGS
{
    DWORD cbSize;
};

struct BA_ONPAUSEAUTOMATICUPDATESBEGIN_RESULTS
{
    DWORD cbSize;
};

struct BA_ONPAUSEAUTOMATICUPDATESCOMPLETE_ARGS
{
    DWORD cbSize;
    HRESULT hrStatus;
};

struct BA_ONPAUSEAUTOMATICUPDATESCOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONPLANBEGIN_ARGS
{
    DWORD cbSize;
    DWORD cPackages;
};

struct BA_ONPLANBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONPLANCOMPATIBLEMSIPACKAGEBEGIN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    LPCWSTR wzCompatiblePackageId;
    LPCWSTR wzCompatiblePackageVersion;
    BOOL fRecommendedRemove;
};

struct BA_ONPLANCOMPATIBLEMSIPACKAGEBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
    BOOL fRequestRemove;
};

struct BA_ONPLANCOMPATIBLEMSIPACKAGECOMPLETE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    LPCWSTR wzCompatiblePackageId;
    HRESULT hrStatus;
    BOOL fRequestedRemove;
};

struct BA_ONPLANCOMPATIBLEMSIPACKAGECOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONPLANCOMPLETE_ARGS
{
    DWORD cbSize;
    HRESULT hrStatus;
};

struct BA_ONPLANCOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONPLANFORWARDCOMPATIBLEBUNDLE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzBundleId;
    BOOTSTRAPPER_RELATION_TYPE relationType;
    LPCWSTR wzBundleTag;
    BOOL fPerMachine;
    LPCWSTR wzVersion;
    BOOL fRecommendedIgnoreBundle;
};

struct BA_ONPLANFORWARDCOMPATIBLEBUNDLE_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
    BOOL fIgnoreBundle;
};

struct BA_ONPLANMSIFEATURE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    LPCWSTR wzFeatureId;
    BOOTSTRAPPER_FEATURE_STATE recommendedState;
};

struct BA_ONPLANMSIFEATURE_RESULTS
{
    DWORD cbSize;
    BOOTSTRAPPER_FEATURE_STATE requestedState;
    BOOL fCancel;
};

struct BA_ONPLANMSIPACKAGE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    BOOL fExecute; // false means rollback.
    BOOTSTRAPPER_ACTION_STATE action;
    BOOTSTRAPPER_MSI_FILE_VERSIONING recommendedFileVersioning;
};

struct BA_ONPLANMSIPACKAGE_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
    BURN_MSI_PROPERTY actionMsiProperty;
    INSTALLUILEVEL uiLevel;
    BOOL fDisableExternalUiHandler;
    BOOTSTRAPPER_MSI_FILE_VERSIONING fileVersioning;
};

struct BA_ONPLANNEDCOMPATIBLEPACKAGE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    LPCWSTR wzCompatiblePackageId;
    BOOL fRemove;
};

struct BA_ONPLANNEDCOMPATIBLEPACKAGE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONPLANNEDPACKAGE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    BOOTSTRAPPER_ACTION_STATE execute;
    BOOTSTRAPPER_ACTION_STATE rollback;
    BOOL fPlannedCache;
    BOOL fPlannedUncache;
};

struct BA_ONPLANNEDPACKAGE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONPLANPACKAGEBEGIN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    BOOTSTRAPPER_PACKAGE_STATE state;
    BOOL fCached;
    BOOTSTRAPPER_PACKAGE_CONDITION_RESULT installCondition;
    BOOTSTRAPPER_REQUEST_STATE recommendedState;
    BOOTSTRAPPER_CACHE_TYPE recommendedCacheType;
    BOOTSTRAPPER_PACKAGE_CONDITION_RESULT repairCondition;
};

struct BA_ONPLANPACKAGEBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
    BOOTSTRAPPER_REQUEST_STATE requestedState;
    BOOTSTRAPPER_CACHE_TYPE requestedCacheType;
};

struct BA_ONPLANPACKAGECOMPLETE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    HRESULT hrStatus;
    BOOTSTRAPPER_REQUEST_STATE requested;
};

struct BA_ONPLANPACKAGECOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONPLANRELATEDBUNDLE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzBundleId;
    BOOTSTRAPPER_REQUEST_STATE recommendedState;
};

struct BA_ONPLANRELATEDBUNDLE_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
    BOOTSTRAPPER_REQUEST_STATE requestedState;
};

struct BA_ONPLANRELATEDBUNDLETYPE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzBundleId;
    BOOTSTRAPPER_RELATED_BUNDLE_PLAN_TYPE recommendedType;
};

struct BA_ONPLANRELATEDBUNDLETYPE_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
    BOOTSTRAPPER_RELATED_BUNDLE_PLAN_TYPE requestedType;
};

struct BA_ONPLANRESTORERELATEDBUNDLE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzBundleId;
    BOOTSTRAPPER_REQUEST_STATE recommendedState;
};

struct BA_ONPLANRESTORERELATEDBUNDLE_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
    BOOTSTRAPPER_REQUEST_STATE requestedState;
};

struct BA_ONPLANMSITRANSACTION_ARGS
{
    DWORD cbSize;
    LPCWSTR wzTransactionId;
};

struct BA_ONPLANMSITRANSACTION_RESULTS
{
    DWORD cbSize;
    BOOL fTransaction;
    BOOL fCancel;
};

struct BA_ONPLANMSITRANSACTIONCOMPLETE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzTransactionId;
    DWORD dwPackagesInTransaction;
    BOOL fPlanned;
};

struct BA_ONPLANMSITRANSACTIONCOMPLETE_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONPLANPATCHTARGET_ARGS
{
    DWORD cbSize;
    LPCWSTR wzPackageId;
    LPCWSTR wzProductCode;
    BOOTSTRAPPER_REQUEST_STATE recommendedState;
};

struct BA_ONPLANPATCHTARGET_RESULTS
{
    DWORD cbSize;
    BOOTSTRAPPER_REQUEST_STATE requestedState;
    BOOL fCancel;
};

struct BA_ONPROGRESS_ARGS
{
    DWORD cbSize;
    DWORD dwProgressPercentage;
    DWORD dwOverallPercentage;
};

struct BA_ONPROGRESS_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
};

struct BA_ONREGISTERBEGIN_ARGS
{
    DWORD cbSize;
    BOOTSTRAPPER_REGISTRATION_TYPE recommendedRegistrationType;
};

struct BA_ONREGISTERBEGIN_RESULTS
{
    DWORD cbSize;
    BOOL fCancel;
    BOOTSTRAPPER_REGISTRATION_TYPE registrationType;
};

struct BA_ONREGISTERCOMPLETE_ARGS
{
    DWORD cbSize;
    HRESULT hrStatus;
};

struct BA_ONREGISTERCOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONROLLBACKMSITRANSACTIONBEGIN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzTransactionId;
};

struct BA_ONROLLBACKMSITRANSACTIONBEGIN_RESULTS
{
    DWORD cbSize;
};

struct BA_ONROLLBACKMSITRANSACTIONCOMPLETE_ARGS
{
    DWORD cbSize;
    LPCWSTR wzTransactionId;
    HRESULT hrStatus;
    BOOTSTRAPPER_APPLY_RESTART restart;
    BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION recommendation;
};

struct BA_ONROLLBACKMSITRANSACTIONCOMPLETE_RESULTS
{
    DWORD cbSize;
    BOOTSTRAPPER_EXECUTEMSITRANSACTIONCOMPLETE_ACTION action;
};

struct BA_ONSETUPDATEBEGIN_ARGS
{
    DWORD cbSize;
};

struct BA_ONSETUPDATEBEGIN_RESULTS
{
    DWORD cbSize;
};

struct BA_ONSETUPDATECOMPLETE_ARGS
{
    DWORD cbSize;
    HRESULT hrStatus;
    LPCWSTR wzPreviousPackageId;
    LPCWSTR wzNewPackageId;
};

struct BA_ONSETUPDATECOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONSHUTDOWN_ARGS
{
    DWORD cbSize;
};

struct BA_ONSHUTDOWN_RESULTS
{
    DWORD cbSize;
    BOOTSTRAPPER_SHUTDOWN_ACTION action;
};

struct BA_ONSTARTUP_ARGS
{
    DWORD cbSize;
};

struct BA_ONSTARTUP_RESULTS
{
    DWORD cbSize;
};

struct BA_ONSYSTEMRESTOREPOINTBEGIN_ARGS
{
    DWORD cbSize;
};

struct BA_ONSYSTEMRESTOREPOINTBEGIN_RESULTS
{
    DWORD cbSize;
};

struct BA_ONSYSTEMRESTOREPOINTCOMPLETE_ARGS
{
    DWORD cbSize;
    HRESULT hrStatus;
};

struct BA_ONSYSTEMRESTOREPOINTCOMPLETE_RESULTS
{
    DWORD cbSize;
};

struct BA_ONUNREGISTERBEGIN_ARGS
{
    DWORD cbSize;
    BOOTSTRAPPER_REGISTRATION_TYPE recommendedRegistrationType;
};

struct BA_ONUNREGISTERBEGIN_RESULTS
{
    DWORD cbSize;
    BOOTSTRAPPER_REGISTRATION_TYPE registrationType;
};

struct BA_ONUNREGISTERCOMPLETE_ARGS
{
    DWORD cbSize;
    HRESULT hrStatus;
};

struct BA_ONUNREGISTERCOMPLETE_RESULTS
{
    DWORD cbSize;
};



extern "C" typedef HRESULT(WINAPI *PFN_BOOTSTRAPPER_APPLICATION_PROC)(
    __in BOOTSTRAPPER_APPLICATION_MESSAGE message,
    __in const LPVOID pvArgs,
    __inout LPVOID pvResults,
    __in_opt LPVOID pvContext
    );

struct BOOTSTRAPPER_DESTROY_ARGS
{
    DWORD cbSize;
    BOOL fReload;
};

struct BOOTSTRAPPER_DESTROY_RESULTS
{
    DWORD cbSize;
    BOOL fDisableUnloading; // indicates the BA dll must not be unloaded after BootstrapperApplicationDestroy.
};

extern "C" typedef void (WINAPI *PFN_BOOTSTRAPPER_APPLICATION_DESTROY)(
    __in const BOOTSTRAPPER_DESTROY_ARGS* pArgs,
    __inout BOOTSTRAPPER_DESTROY_RESULTS* pResults
    );



struct BOOTSTRAPPER_CREATE_ARGS
{
    DWORD cbSize;
    DWORD64 qwEngineAPIVersion;
    PFN_BOOTSTRAPPER_ENGINE_PROC pfnBootstrapperEngineProc;
    LPVOID pvBootstrapperEngineProcContext;
    BOOTSTRAPPER_COMMAND* pCommand;
};

struct BOOTSTRAPPER_CREATE_RESULTS
{
    DWORD cbSize;
    PFN_BOOTSTRAPPER_APPLICATION_PROC pfnBootstrapperApplicationProc;
    LPVOID pvBootstrapperApplicationProcContext;
};

extern "C" typedef HRESULT(WINAPI *PFN_BOOTSTRAPPER_APPLICATION_CREATE)(
    __in const BOOTSTRAPPER_CREATE_ARGS* pArgs,
    __inout BOOTSTRAPPER_CREATE_RESULTS* pResults
    );
