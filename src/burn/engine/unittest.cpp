// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

#include "precomp.h"


HRESULT UnittestParseFromXml(
    __in BURN_UNIT_TEST_CONTEXT* pUnittestContext,
    __in IXMLDOMNode* pixnBundle
)
{
    HRESULT hr = S_OK;
    BOOL fXmlFound = FALSE;

    // @UnittestPasswordHash (optional attribute on the root BurnManifest element)
    hr = XmlGetAttributeEx(pixnBundle, L"UnittestPasswordHash", &pUnittestContext->wzUnittestPasswordHash);
    ExitOnOptionalXmlQueryFailure(hr, fXmlFound, "Failed to get Bundle/@UnittestPasswordHash.");

LExit:
    return hr;
}

HRESULT UnittestValidatePassword(
    __in BURN_UNIT_TEST_CONTEXT* pUnittestContext,
    __in LPCWSTR wzPassword
)
{
    HRESULT hr = S_OK;
    BYTE rgbHash[SHA512_HASH_LEN] = { };
    LPWSTR sczHashHex = NULL;

    // A bundle compiled without a password hash cannot run unit tests.
    if (!pUnittestContext->wzUnittestPasswordHash || !*pUnittestContext->wzUnittestPasswordHash)
    {
        ExitWithRootFailure(hr, E_ACCESSDENIED, "Unit test password hash is not configured in the bundle manifest.");
    }

    // Hash the provided password using SHA-512, matching the bytes of the wide string (no null terminator).
    hr = CrypHashBuffer(reinterpret_cast<LPCBYTE>(wzPassword), wcslen(wzPassword) * sizeof(WCHAR), PROV_RSA_AES, CALG_SHA_512, rgbHash, sizeof(rgbHash));
    ExitOnFailure(hr, "Failed to hash unit test password.");

    // Encode the hash to uppercase hex so it can be compared to the stored string.
    hr = StrAllocHexEncode(rgbHash, sizeof(rgbHash), &sczHashHex);
    ExitOnFailure(hr, "Failed to hex-encode unit test password hash.");

    // Compare case-insensitively to tolerate lowercase hex in the manifest.
    if (CSTR_EQUAL != ::CompareStringOrdinal(sczHashHex, -1, pUnittestContext->wzUnittestPasswordHash, -1, TRUE))
    {
        ExitWithRootFailure(hr, E_ACCESSDENIED, "Unit test password validation failed.");
    }

LExit:
    SecureZeroMemory(rgbHash, sizeof(rgbHash));
    ReleaseNullStrSecure(sczHashHex);

    return hr;
}

HRESULT UnittestQuit(
    __in BURN_ENGINE_STATE* pEngineState
)
{
    pEngineState->unitTestContext.fUnitTestQuit = TRUE;
    return S_OK;
}
