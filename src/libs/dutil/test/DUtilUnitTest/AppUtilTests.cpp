// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

#include "precomp.h"

namespace DutilTests
{
    using namespace System;
    using namespace Xunit;
    using namespace WixInternal::TestSupport;

    public ref class AppUtil
    {
    public:
        [Fact]
        void WaitForMultipleObjectsTest()
        {
            HRESULT hr = S_OK;
            HANDLE hOne = NULL;
            HANDLE hTwo = NULL;
            HANDLE rghHandles[2] = { };
            DWORD dwSignaledIndex = 0;

            try
            {
                hOne = ::CreateEventW(NULL, TRUE, FALSE, NULL);
                if (!hOne)
                {
                    hr = HRESULT_FROM_WIN32(::GetLastError());
                    NativeAssert::Succeeded(FAILED(hr) ? hr : E_FAIL, "Failed to create event.");
                }

                hTwo = ::CreateEventW(NULL, TRUE, TRUE, NULL);
                if (!hTwo)
                {
                    hr = HRESULT_FROM_WIN32(::GetLastError());
                    NativeAssert::Succeeded(FAILED(hr) ? hr : E_FAIL, "Failed to create event.");
                }

                rghHandles[0] = hOne;
                rghHandles[1] = hTwo;

                hr = AppWaitForMultipleObjects(countof(rghHandles), rghHandles, FALSE, 0, &dwSignaledIndex);
                NativeAssert::Succeeded(hr, "Failed to wait for multiple objects.");
                Assert::Equal<DWORD>(1, dwSignaledIndex);

                rghHandles[0] = hTwo;
                rghHandles[1] = hOne;

                hr = AppWaitForMultipleObjects(countof(rghHandles), rghHandles, FALSE, 0, &dwSignaledIndex);
                NativeAssert::Succeeded(hr, "Failed to wait for multiple objects.");
                Assert::Equal<DWORD>(0, dwSignaledIndex);
            }
            finally
            {
                ReleaseHandle(hOne);
                ReleaseHandle(hTwo);
            }
        }

        [Fact]
        void AppAppendCommandLineArgumentTest()
        {
            HRESULT hr = S_OK;
            LPCWSTR rgszArgs[] = {
                L"a.exe",
                L"a\\",
                L"C:\\My Folder",
                L"C:\\My Folder\\",
                L"C:\\My Folder\\ ",
                L"C:\\My Folder\\\\",
                L"C:\\My Folder\"",
                L"INSTALL_FOLDER=C:\\My Folder\\",
            };
            int rgcArgs = ARRAYSIZE(rgszArgs);
            LPWSTR szCommandLine = nullptr;
            LPWSTR *rgszParsedArgs = nullptr;
            int rgcParsedArgs = 0;

            try
            {
                for (auto i = 0; i < rgcArgs; ++i)
                {
                    hr = AppAppendCommandLineArgument(&szCommandLine, rgszArgs[i]);
                    NativeAssert::Succeeded(hr, "Failed to append command line argument.");
                }

                hr = AppParseCommandLine(szCommandLine, &rgcParsedArgs, &rgszParsedArgs);
                NativeAssert::Succeeded(hr, "Failed to parse command line.");
                NativeAssert::NotNull((LPCWSTR)rgszParsedArgs);
                Assert::Equal<int>(rgcArgs, rgcParsedArgs);

                for (auto i = 0; i < rgcArgs; ++i)
                {
                    NativeAssert::StringEqual(rgszArgs[i], rgszParsedArgs[i]);
                }
            }
            finally
            {
                if (rgszParsedArgs)
                {
                    AppFreeCommandLineArgs(rgszParsedArgs);
                }
                ReleaseStr(szCommandLine);
            }
        }
    };
}
