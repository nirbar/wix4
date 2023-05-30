// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

#include "precomp.h"


// function definitions

EXTERN_C int WINAPI wWinMain(
    __in HINSTANCE hInstance,
    __in_opt HINSTANCE /* hPrevInstance */,
    __in_z_opt LPWSTR /*lpCmdLine*/,
    __in int /*nCmdShow*/
    )
{
    HRESULT hr = S_OK;
    IBootstrapperApplication* pApplication = NULL;

#ifdef DEBUGGABLE_RELEASE
    DWORD dwSize = ::GetEnvironmentVariableW(L"DEBUG_WIXUIBA", nullptr, 0);
    if (dwSize > 0)
    {
        ::MessageBoxW(NULL, L"DEBUG_WIXUIBA environment variable is set. You may attach a debugger now.\nTo disable this message, delete the environment variable 'DEBUG_WIXUIBA'", L"DEBUG_WIXUIBA", MB_OK);
    }
#endif

    hr = CreateWixInternalUIBootstrapperApplication(hInstance, &pApplication);
    ExitOnFailure(hr, "Failed to create WiX internal UI bootstrapper application.");

    hr = BootstrapperApplicationRun(pApplication);
    ExitOnFailure(hr, "Failed to run WiX internal UI bootstrapper application.");

LExit:
    ReleaseObject(pApplication);

    return 0;
}
