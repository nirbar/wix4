// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

#include "precomp.h"


static HRESULT SendRequiredBextMessage(
    __in BURN_EXTENSION* pExtension,
    __in BUNDLE_EXTENSION_MESSAGE message,
    __in const LPVOID pvArgs,
    __inout LPVOID pvResults
    );

// function definitions

/*******************************************************************
 BurnExtensionParseFromXml -

*******************************************************************/
EXTERN_C HRESULT BurnExtensionParseFromXml(
    __in BURN_EXTENSIONS* pBurnExtensions,
    __in BURN_PAYLOADS* pBaPayloads,
    __in IXMLDOMNode* pixnBundle
    )
{
    HRESULT hr = S_OK;
    IXMLDOMNodeList* pixnNodes = NULL;
    IXMLDOMNode* pixnNode = NULL;
    DWORD cNodes = 0;
    LPWSTR scz = NULL;

    // Select BundleExtension nodes.
    hr = XmlSelectNodes(pixnBundle, L"BundleExtension", &pixnNodes);
    ExitOnFailure(hr, "Failed to select BundleExtension nodes.");

    // Get BundleExtension node count.
    hr = pixnNodes->get_length((long*)&cNodes);
    ExitOnFailure(hr, "Failed to get BundleExtension node count.");

    if (!cNodes)
    {
        ExitFunction();
    }

    // Allocate memory for BundleExtensions.
    pBurnExtensions->rgExtensions = (BURN_EXTENSION*)MemAlloc(sizeof(BURN_EXTENSION) * cNodes, TRUE);
    ExitOnNull(pBurnExtensions->rgExtensions, hr, E_OUTOFMEMORY, "Failed to allocate memory for BundleExtension structs.");

    pBurnExtensions->cExtensions = cNodes;

    // parse search elements
    for (DWORD i = 0; i < cNodes; ++i)
    {
        BURN_EXTENSION* pExtension = &pBurnExtensions->rgExtensions[i];

        hr = XmlNextElement(pixnNodes, &pixnNode, NULL);
        ExitOnFailure(hr, "Failed to get next node.");

        // @Id
        hr = XmlGetAttributeEx(pixnNode, L"Id", &pExtension->sczId);
        ExitOnFailure(hr, "Failed to get @Id.");

        // @EntryPayloadId
        hr = XmlGetAttributeEx(pixnNode, L"EntryPayloadSourcePath", &scz);
        ExitOnFailure(hr, "Failed to get @EntryPayloadSourcePath.");

        hr = PayloadFindEmbeddedBySourcePath(pBaPayloads->sdhPayloads, scz, &pExtension->pEntryPayload);
        ExitOnFailure(hr, "Failed to find BundleExtension EntryPayload '%ls'.", pExtension->sczId);

        // prepare next iteration
        ReleaseNullObject(pixnNode);
    }

    hr = S_OK;

LExit:
    ReleaseStr(scz);
    ReleaseObject(pixnNode);
    ReleaseObject(pixnNodes);

    return hr;
}

/*******************************************************************
 BurnExtensionUninitialize -

*******************************************************************/
EXTERN_C void BurnExtensionUninitialize(
    __in BURN_EXTENSIONS* pBurnExtensions
    )
{
    if (pBurnExtensions->rgExtensions)
    {
        for (DWORD i = 0; i < pBurnExtensions->cExtensions; ++i)
        {
            BURN_EXTENSION* pExtension = &pBurnExtensions->rgExtensions[i];

            ReleaseStr(pExtension->sczId);
        }
        MemFree(pBurnExtensions->rgExtensions);
    }

    // clear struct
    memset(pBurnExtensions, 0, sizeof(BURN_EXTENSIONS));
}

/*******************************************************************
 BurnExtensionLoad -

*******************************************************************/
EXTERN_C HRESULT BurnExtensionLoad(
    __in BURN_EXTENSIONS * pBurnExtensions,
    __in BURN_EXTENSION_ENGINE_CONTEXT* pEngineContext
    )
{
    HRESULT hr = S_OK;
    LPWSTR sczBundleExtensionDataPath = NULL;
    BUNDLE_EXTENSION_CREATE_ARGS args = { };
    BUNDLE_EXTENSION_CREATE_RESULTS results = { };

    if (!pBurnExtensions->rgExtensions || !pBurnExtensions->cExtensions)
    {
        ExitFunction();
    }

    hr = PathConcat(pEngineContext->pEngineState->userExperience.sczTempDirectory, L"BundleExtensionData.xml", &sczBundleExtensionDataPath);
    ExitOnFailure(hr, "Failed to get BundleExtensionDataPath.");

    for (DWORD i = 0; i < pBurnExtensions->cExtensions; ++i)
    {
        BURN_EXTENSION* pExtension = &pBurnExtensions->rgExtensions[i];

        memset(&args, 0, sizeof(BUNDLE_EXTENSION_CREATE_ARGS));
        memset(&results, 0, sizeof(BUNDLE_EXTENSION_CREATE_RESULTS));

        args.cbSize = sizeof(BUNDLE_EXTENSION_CREATE_ARGS);
        args.pfnBundleExtensionEngineProc = EngineForExtensionProc;
        args.pvBundleExtensionEngineProcContext = pEngineContext;
        args.qwEngineAPIVersion = MAKEQWORDVERSION(2021, 4, 27, 0);
        args.wzBootstrapperWorkingFolder = pEngineContext->pEngineState->userExperience.sczTempDirectory;
        args.wzBundleExtensionDataPath = sczBundleExtensionDataPath;
        args.wzExtensionId = pExtension->sczId;

        results.cbSize = sizeof(BUNDLE_EXTENSION_CREATE_RESULTS);

        // Load BundleExtension DLL.
        pExtension->hBextModule = ::LoadLibraryExW(pExtension->pEntryPayload->sczLocalFilePath, NULL, LOAD_WITH_ALTERED_SEARCH_PATH);
        ExitOnNullWithLastError(pExtension->hBextModule, hr, "Failed to load BundleExtension DLL '%ls': '%ls'.", pExtension->sczId, pExtension->pEntryPayload->sczLocalFilePath);

        // Get BundleExtensionCreate entry-point.
        PFN_BUNDLE_EXTENSION_CREATE pfnCreate = (PFN_BUNDLE_EXTENSION_CREATE)::GetProcAddress(pExtension->hBextModule, "BundleExtensionCreate");
        ExitOnNullWithLastError(pfnCreate, hr, "Failed to get BundleExtensionCreate entry-point '%ls'.", pExtension->sczId);

        // Create BundleExtension.
        hr = pfnCreate(&args, &results);
        ExitOnFailure(hr, "Failed to create BundleExtension '%ls'.", pExtension->sczId);

        pExtension->pfnBurnExtensionProc = results.pfnBundleExtensionProc;
        pExtension->pvBurnExtensionProcContext = results.pvBundleExtensionProcContext;
    }

LExit:
    ReleaseStr(sczBundleExtensionDataPath);

    return hr;
}

/*******************************************************************
 BurnExtensionUnload -

*******************************************************************/
EXTERN_C void BurnExtensionUnload(
    __in BURN_EXTENSIONS * pBurnExtensions
    )
{
    HRESULT hr = S_OK;

    if (pBurnExtensions->rgExtensions)
    {
        for (DWORD i = 0; i < pBurnExtensions->cExtensions; ++i)
        {
            BURN_EXTENSION* pExtension = &pBurnExtensions->rgExtensions[i];

            if (pExtension->hBextModule)
            {
                // Get BundleExtensionDestroy entry-point and call it if it exists.
                PFN_BUNDLE_EXTENSION_DESTROY pfnDestroy = (PFN_BUNDLE_EXTENSION_DESTROY)::GetProcAddress(pExtension->hBextModule, "BundleExtensionDestroy");
                if (pfnDestroy)
                {
                    pfnDestroy();
                }

                // Free BundleExtension DLL.
                if (!::FreeLibrary(pExtension->hBextModule))
                {
                    hr = HRESULT_FROM_WIN32(::GetLastError());
                    TraceError(hr, "Failed to unload BundleExtension DLL.");
                }
                pExtension->hBextModule = NULL;
            }
        }
    }
}

EXTERN_C HRESULT BurnExtensionFindById(
    __in BURN_EXTENSIONS* pBurnExtensions,
    __in_z LPCWSTR wzId,
    __out BURN_EXTENSION** ppExtension
    )
{
    HRESULT hr = S_OK;
    BURN_EXTENSION* pExtension = NULL;

    for (DWORD i = 0; i < pBurnExtensions->cExtensions; ++i)
    {
        pExtension = &pBurnExtensions->rgExtensions[i];

        if (CSTR_EQUAL == ::CompareStringW(LOCALE_INVARIANT, 0, pExtension->sczId, -1, wzId, -1))
        {
            *ppExtension = pExtension;
            ExitFunction1(hr = S_OK);
        }
    }

    hr = E_NOTFOUND;

LExit:
    return hr;
}

EXTERN_C BEEAPI BurnExtensionPerformSearch(
    __in BURN_EXTENSION* pExtension,
    __in LPWSTR wzSearchId,
    __in LPWSTR wzVariable
    )
{
    HRESULT hr = S_OK;
    BUNDLE_EXTENSION_SEARCH_ARGS args = { };
    BUNDLE_EXTENSION_SEARCH_RESULTS results = { };

    args.cbSize = sizeof(args);
    args.wzId = wzSearchId;
    args.wzVariable = wzVariable;

    results.cbSize = sizeof(results);

    hr = SendRequiredBextMessage(pExtension, BUNDLE_EXTENSION_MESSAGE_SEARCH, &args, &results);
    ExitOnFailure(hr, "BundleExtension '%ls' Search '%ls' failed.", pExtension->sczId, wzSearchId);

LExit:
    return hr;
}

EXTERN_C BEEAPI BurnExtensionContainerOpen(
    __in BURN_EXTENSION* pExtension,
    __in LPCWSTR wzContainerId,
    __in LPCWSTR wzFilePath,
    __in BURN_CONTAINER_CONTEXT* pContext
)
{
    HRESULT hr = S_OK;
    BUNDLE_EXTENSION_CONTAINER_OPEN_ARGS args = { };
    BUNDLE_EXTENSION_CONTAINER_OPEN_RESULTS results = { };

    args.cbSize = sizeof(args);
    args.wzContainerId = wzContainerId;
    args.wzFilePath = wzFilePath;

    results.cbSize = sizeof(results);

    hr = SendRequiredBextMessage(pExtension, BUNDLE_EXTENSION_MESSAGE_CONTAINER_OPEN, &args, &results);
    ExitOnFailure(hr, "BundleExtension '%ls' open container '%ls' failed.", pExtension->sczId, wzFilePath);

    pContext->Bex.pExtensionContext = results.pContext;

LExit:
    return hr;
}

EXTERN_C BEEAPI BurnExtensionContainerOpenAttached(
    __in BURN_EXTENSION* pExtension,
    __in LPCWSTR wzContainerId,
    __in HANDLE hBundle,
    __in DWORD64 qwContainerStartPos,
    __in DWORD64 qwContainerSize,
    __in BURN_CONTAINER_CONTEXT* pContext
)
{
    HRESULT hr = S_OK;
    BUNDLE_EXTENSION_CONTAINER_OPEN_ATTACHED_ARGS args = { };
    BUNDLE_EXTENSION_CONTAINER_OPEN_ATTACHED_RESULTS results = { };

    args.cbSize = sizeof(args);
    args.wzContainerId = wzContainerId;
    args.hBundle = hBundle;
    args.qwContainerStartPos = qwContainerStartPos;
    args.qwContainerSize = qwContainerSize;

    results.cbSize = sizeof(results);

    hr = SendRequiredBextMessage(pExtension, BUNDLE_EXTENSION_MESSAGE_CONTAINER_OPEN_ATTACHED, &args, &results);
    ExitOnFailure(hr, "BundleExtension '%ls' open attached container failed.", pExtension->sczId);

    pContext->Bex.pExtensionContext = results.pContext;

LExit:
    return hr;
}

BEEAPI BurnExtensionContainerNextStream(
    __in BURN_EXTENSION* pExtension,
    __in BURN_CONTAINER_CONTEXT* pContext,
    __inout_z LPWSTR* psczStreamName
)
{
    HRESULT hr = S_OK;
    BUNDLE_EXTENSION_CONTAINER_NEXT_STREAM_ARGS args = { };
    BUNDLE_EXTENSION_CONTAINER_NEXT_STREAM_RESULTS results = { };
    BSTR sczStreamName = nullptr;

    if (psczStreamName && *psczStreamName)
    {
        sczStreamName = ::SysAllocString(*psczStreamName);
        ExitOnNull(sczStreamName, hr, E_FAIL, "Failed to allocate sys string");
    }

    args.cbSize = sizeof(args);
    args.pContext = pContext->Bex.pExtensionContext;

    results.cbSize = sizeof(results);
    results.psczStreamName = &sczStreamName;

    hr = SendRequiredBextMessage(pExtension, BUNDLE_EXTENSION_MESSAGE_CONTAINER_NEXT_STREAM, &args, &results);
    if (hr != E_NOMOREITEMS)
    {
        ExitOnFailure(hr, "BundleExtension '%ls' failed to move to next stream.", pExtension->sczId);

        if (psczStreamName)
        {
            if (sczStreamName)
            {
                hr = StrAllocString(psczStreamName, sczStreamName, 0);
                ExitOnFailure(hr, "Failed to copy string");
            }
            else
            {
                ReleaseNullStr(*psczStreamName);
            }
        }
    }

LExit:
    if (sczStreamName)
    {
        ::SysFreeString(sczStreamName);
    }

    return hr;
}

BEEAPI BurnExtensionContainerStreamToFile(
    __in BURN_EXTENSION* pExtension,
    __in BURN_CONTAINER_CONTEXT* pContext,
    __in_z LPCWSTR wzFileName
)
{
    HRESULT hr = S_OK;
    BUNDLE_EXTENSION_CONTAINER_STREAM_TO_FILE_ARGS args = { };
    BUNDLE_EXTENSION_CONTAINER_STREAM_TO_FILE_RESULTS results = { };

    args.cbSize = sizeof(args);
    args.pContext = pContext->Bex.pExtensionContext;
    args.wzFileName = wzFileName;

    results.cbSize = sizeof(results);

    hr = SendRequiredBextMessage(pExtension, BUNDLE_EXTENSION_MESSAGE_CONTAINER_STREAM_TO_FILE, &args, &results);
    ExitOnFailure(hr, "BundleExtension '%ls' failed to extract file '%ls'.", pExtension->sczId, wzFileName);

LExit:
    return hr;
}

BEEAPI BurnExtensionContainerStreamToBuffer(
    __in BURN_EXTENSION* pExtension,
    __in BURN_CONTAINER_CONTEXT* pContext,
    __inout LPBYTE* ppbBuffer,
    __inout SIZE_T* pcbBuffer
)
{
    HRESULT hr = S_OK;
    BUNDLE_EXTENSION_CONTAINER_STREAM_TO_BUFFER_ARGS args = { };
    BUNDLE_EXTENSION_CONTAINER_STREAM_TO_BUFFER_RESULTS results = { };
    LPBYTE pbBuffer = nullptr;
    SIZE_T cbBuffer = 0;
    errno_t err = 0;

    args.cbSize = sizeof(args);
    args.pContext = pContext->Bex.pExtensionContext;

    results.cbSize = sizeof(results);
    results.ppbBuffer = &pbBuffer;
    results.pcbBuffer = &cbBuffer;

    hr = SendRequiredBextMessage(pExtension, BUNDLE_EXTENSION_MESSAGE_CONTAINER_STREAM_TO_BUFFER, &args, &results);
    ExitOnFailure(hr, "BundleExtension '%ls' failed to extract stream to buffer.", pExtension->sczId);

    if (pbBuffer)
    {
        if (ppbBuffer && *ppbBuffer)
        {
            LPVOID pv = MemReAlloc(*ppbBuffer, cbBuffer, FALSE);
            ExitOnNull(pv, hr, E_OUTOFMEMORY, "Failed to reallocate memory.");

            *ppbBuffer = (LPBYTE)pv;
            *pcbBuffer = cbBuffer;
        }
        else
        {
            *ppbBuffer = (LPBYTE)MemAlloc(cbBuffer, FALSE);
            ExitOnNull(*ppbBuffer, hr, E_OUTOFMEMORY, "Failed to allocate memory.");

            *pcbBuffer = cbBuffer;
        }

        err = ::memcpy_s(*ppbBuffer, cbBuffer, pbBuffer, cbBuffer);
        ExitOnNull(!err, hr, HRESULT_FROM_WIN32(err), "Failed to copy memory");
    }
    else if (ppbBuffer && *ppbBuffer)
    {
        ReleaseNullMem(*ppbBuffer);
        *pcbBuffer = 0;
    }

LExit:
    if (pbBuffer)
    {
        ::CoTaskMemFree(pbBuffer);
    }

    return hr;
}

BEEAPI BurnExtensionContainerSkipStream(
    __in BURN_EXTENSION* pExtension,
    __in BURN_CONTAINER_CONTEXT* pContext
)
{
    HRESULT hr = S_OK;
    BUNDLE_EXTENSION_CONTAINER_STREAM_TO_BUFFER_ARGS args = { };
    BUNDLE_EXTENSION_CONTAINER_STREAM_TO_BUFFER_RESULTS results = { };

    args.cbSize = sizeof(args);
    args.pContext = pContext->Bex.pExtensionContext;

    results.cbSize = sizeof(results);

    hr = SendRequiredBextMessage(pExtension, BUNDLE_EXTENSION_MESSAGE_CONTAINER_SKIP_STREAM, &args, &results);
    ExitOnFailure(hr, "BundleExtension '%ls' failed to skip stream.", pExtension->sczId);

LExit:
    return hr;
}

BEEAPI BurnExtensionContainerClose(
    __in BURN_EXTENSION* pExtension,
    __in BURN_CONTAINER_CONTEXT* pContext
)
{
    HRESULT hr = S_OK;
    BUNDLE_EXTENSION_CONTAINER_CLOSE_ARGS args = { };
    BUNDLE_EXTENSION_CONTAINER_CLOSE_RESULTS results = { };

    args.cbSize = sizeof(args);
    args.pContext = pContext->Bex.pExtensionContext;

    results.cbSize = sizeof(results);

    hr = SendRequiredBextMessage(pExtension, BUNDLE_EXTENSION_MESSAGE_CONTAINER_CLOSE, &args, &results);
    ExitOnFailure(hr, "BundleExtension '%ls' failed to close container.", pExtension->sczId);

LExit:
    return hr;
}

static HRESULT SendRequiredBextMessage(
    __in BURN_EXTENSION* pExtension,
    __in BUNDLE_EXTENSION_MESSAGE message,
    __in const LPVOID pvArgs,
    __inout LPVOID pvResults
    )
{
    HRESULT hr = S_OK;

    hr = pExtension->pfnBurnExtensionProc(message, pvArgs, pvResults, pExtension->pvBurnExtensionProcContext);

    return hr;
}
