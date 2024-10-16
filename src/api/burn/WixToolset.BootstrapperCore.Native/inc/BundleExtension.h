#pragma once
// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.


#if defined(__cplusplus)
extern "C" {
#endif

enum BUNDLE_EXTENSION_MESSAGE
{
    BUNDLE_EXTENSION_MESSAGE_SEARCH,
    BUNDLE_EXTENSION_MESSAGE_CONTAINER_OPEN,
    BUNDLE_EXTENSION_MESSAGE_CONTAINER_OPEN_ATTACHED,
    BUNDLE_EXTENSION_MESSAGE_CONTAINER_NEXT_STREAM,
    BUNDLE_EXTENSION_MESSAGE_CONTAINER_STREAM_TO_FILE,
    BUNDLE_EXTENSION_MESSAGE_CONTAINER_STREAM_TO_BUFFER,
    BUNDLE_EXTENSION_MESSAGE_CONTAINER_SKIP_STREAM,
    BUNDLE_EXTENSION_MESSAGE_CONTAINER_CLOSE,
};

typedef struct _BUNDLE_EXTENSION_SEARCH_ARGS
{
    DWORD cbSize;
    LPCWSTR wzId;
    LPCWSTR wzVariable;
} BUNDLE_EXTENSION_SEARCH_ARGS;

typedef struct _BUNDLE_EXTENSION_SEARCH_RESULTS
{
    DWORD cbSize;
} BUNDLE_EXTENSION_SEARCH_RESULTS;


// Container ops arguments
typedef struct _BUNDLE_EXTENSION_CONTAINER_OPEN_ARGS
{
    DWORD cbSize;
    LPCWSTR wzContainerId;
    LPCWSTR wzFilePath;
} BUNDLE_EXTENSION_CONTAINER_OPEN_ARGS;

typedef struct _BUNDLE_EXTENSION_CONTAINER_OPEN_RESULTS
{
    DWORD cbSize;
    LPVOID pContext;
} BUNDLE_EXTENSION_CONTAINER_OPEN_RESULTS;

typedef struct _BUNDLE_EXTENSION_CONTAINER_OPEN_ATTACHED_ARGS
{
    DWORD cbSize;
    LPCWSTR wzContainerId;
    HANDLE hBundle;
    DWORD64 qwContainerStartPos;
    DWORD64 qwContainerSize;
} BUNDLE_EXTENSION_CONTAINER_OPEN_ATTACHED_ARGS;

typedef struct _BUNDLE_EXTENSION_CONTAINER_OPEN_ATTACHED_RESULTS
{
    DWORD cbSize;
    LPVOID pContext;
} BUNDLE_EXTENSION_CONTAINER_OPEN_ATTACHED_RESULTS;

typedef struct _BUNDLE_EXTENSION_CONTAINER_NEXT_STREAM_ARGS
{
    DWORD cbSize;
    LPVOID pContext;
} BUNDLE_EXTENSION_CONTAINER_NEXT_STREAM_ARGS;

typedef struct _BUNDLE_EXTENSION_CONTAINER_NEXT_STREAM_RESULTS
{
    DWORD cbSize;
    // String allocated using SysAllocString on input, expected to be allocated using same method on return
    BSTR *psczStreamName;
} BUNDLE_EXTENSION_CONTAINER_NEXT_STREAM_RESULTS;

typedef struct _BUNDLE_EXTENSION_CONTAINER_STREAM_TO_FILE_ARGS
{
    DWORD cbSize;
    LPVOID pContext;
    LPCWSTR wzFileName;
} BUNDLE_EXTENSION_CONTAINER_STREAM_TO_FILE_ARGS;

typedef struct _BUNDLE_EXTENSION_CONTAINER_STREAM_TO_FILE_RESULTS
{
    DWORD cbSize;
} BUNDLE_EXTENSION_CONTAINER_STREAM_TO_FILE_RESULTS;

typedef struct _BUNDLE_EXTENSION_CONTAINER_STREAM_TO_BUFFER_ARGS
{
    DWORD cbSize;
    LPVOID pContext;
} BUNDLE_EXTENSION_CONTAINER_STREAM_TO_BUFFER_ARGS;

typedef struct _BUNDLE_EXTENSION_CONTAINER_STREAM_TO_BUFFER_RESULTS
{
    DWORD cbSize;
    // Buffer must be allocated with CoTaskMemAlloc()
    LPBYTE *ppbBuffer;
    SIZE_T *pcbBuffer;
} BUNDLE_EXTENSION_CONTAINER_STREAM_TO_BUFFER_RESULTS;

typedef struct _BUNDLE_EXTENSION_CONTAINER_SKIP_STREAM_ARGS
{
    DWORD cbSize;
    LPVOID pContext;
} BUNDLE_EXTENSION_CONTAINER_SKIP_STREAM_ARGS;

typedef struct _BUNDLE_EXTENSION_CONTAINER_SKIP_STREAM_RESULTS
{
    DWORD cbSize;
} BUNDLE_EXTENSION_CONTAINER_SKIP_STREAM_RESULTS;

typedef struct _BUNDLE_EXTENSION_CONTAINER_CLOSE_ARGS
{
    DWORD cbSize;
    LPVOID pContext;
} BUNDLE_EXTENSION_CONTAINER_CLOSE_ARGS;

typedef struct _BUNDLE_EXTENSION_CONTAINER_CLOSE_RESULTS
{
    DWORD cbSize;
} BUNDLE_EXTENSION_CONTAINER_CLOSE_RESULTS;

extern "C" typedef HRESULT(WINAPI *PFN_BUNDLE_EXTENSION_PROC)(
    __in BUNDLE_EXTENSION_MESSAGE message,
    __in const LPVOID pvArgs,
    __inout LPVOID pvResults,
    __in_opt LPVOID pvContext
    );

typedef struct _BUNDLE_EXTENSION_CREATE_ARGS
{
    DWORD cbSize;
    DWORD64 qwEngineAPIVersion;
    PFN_BUNDLE_EXTENSION_ENGINE_PROC pfnBundleExtensionEngineProc;
    LPVOID pvBundleExtensionEngineProcContext;
    LPCWSTR wzBootstrapperWorkingFolder;
    LPCWSTR wzBundleExtensionDataPath;
    LPCWSTR wzExtensionId;
} BUNDLE_EXTENSION_CREATE_ARGS;

typedef struct _BUNDLE_EXTENSION_CREATE_RESULTS
{
    DWORD cbSize;
    PFN_BUNDLE_EXTENSION_PROC pfnBundleExtensionProc;
    LPVOID pvBundleExtensionProcContext;
} BUNDLE_EXTENSION_CREATE_RESULTS;

extern "C" typedef HRESULT(WINAPI *PFN_BUNDLE_EXTENSION_CREATE)(
    __in const BUNDLE_EXTENSION_CREATE_ARGS* pArgs,
    __inout BUNDLE_EXTENSION_CREATE_RESULTS* pResults
    );

extern "C" typedef void (WINAPI *PFN_BUNDLE_EXTENSION_DESTROY)();

#if defined(__cplusplus)
}
#endif
