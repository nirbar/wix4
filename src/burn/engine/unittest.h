// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.
#pragma once


// Private unit-test protocol types.
// These are intentionally NOT part of the public BootstrapperApplicationTypes.h / BootstrapperEngineTypes.h APIs.
// The test framework must duplicate these definitions; they will never change without a corresponding
// update to the test framework.

// Engine → test-host (BA pipe direction): sent to notify the test host of the real BA path.
// High value to avoid collision with any future public BOOTSTRAPPER_APPLICATION_MESSAGE values.
const DWORD UNITTEST_APPLICATION_MESSAGE_START_REAL_BA = 0x0FFFFFFF - BOOTSTRAPPER_APPLICATION_MESSAGE_UNKNOWN;

struct BA_ONUNITTESTAPPLICATIONSTARTREALBA_ARGS
{
    DWORD dwApiVersion;
    LPCWSTR wzBootstrapperApplicationPath;
    int nCmdShow;
};

struct BA_ONUNITTESTAPPLICATIONSTARTREALBA_RESULTS
{
    DWORD dwApiVersion;
};

// Test host → engine (BAEngine pipe direction): sent by the test host when all unit tests have completed.
// High value to avoid collision with any future public BOOTSTRAPPER_ENGINE_MESSAGE values.
const DWORD UNITTEST_ENGINE_MESSAGE_QUIT = 0x0FFFFFFF - BOOTSTRAPPER_ENGINE_MESSAGE_UNKNOWN;

struct BAENGINE_UNITTESTQUIT_ARGS
{
    DWORD dwApiVersion;
    DWORD dwExitCode;
};

struct BAENGINE_UNITTESTQUIT_RESULTS
{
    DWORD dwApiVersion;
};


#if defined(__cplusplus)
extern "C" {
#endif


// function declarations

HRESULT UnittestParseFromXml(
    __in BURN_UNIT_TEST_CONTEXT* pUnittestContext,
    __in IXMLDOMNode* pixnBundle
    );

HRESULT UnittestValidatePassword(
    __in BURN_UNIT_TEST_CONTEXT* pUnittestContext
    );

HRESULT UnittestQuit(
    __in BURN_ENGINE_STATE* pEngineState
    );

#if defined(__cplusplus)
}
#endif
