// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

#include "precomp.h"

static const DWORD PIPE_64KB = 64 * 1024;
static const DWORD PIPE_WAIT_FOR_CONNECTION = 100;   // wait a 10th of a second,
static const DWORD PIPE_RETRY_FOR_CONNECTION = 1800; // for up to 3 minutes.

static const LPCWSTR PIPE_NAME_FORMAT_STRING = L"\\\\.\\pipe\\%ls";
static const LPCWSTR CACHE_PIPE_NAME_FORMAT_STRING = L"\\\\.\\pipe\\%ls.Cache";
static const LPCWSTR LOGGING_PIPE_NAME_FORMAT_STRING = L"\\\\.\\pipe\\%ls.Log";

static HRESULT AllocatePipeMessage(
    __in DWORD dwMessage,
    __in_bcount_opt(cbData) LPVOID pvData,
    __in SIZE_T cbData,
    __out_bcount(cb) LPVOID* ppvMessage,
    __out SIZE_T* pcbMessage
    );
static void FreePipeMessage(
    __in BURN_PIPE_MESSAGE *pMsg
    );
static HRESULT WritePipeMessage(
    __in HANDLE hPipe,
    __in DWORD dwMessage,
    __in_bcount_opt(cbData) LPVOID pvData,
    __in SIZE_T cbData
    );
static HRESULT GetPipeMessage(
    __in HANDLE hPipe,
    __in BURN_PIPE_MESSAGE* pMsg
    );
static HRESULT ChildPipeConnected(
    __in HANDLE hPipe,
    __in_z LPCWSTR wzSecret,
    __inout DWORD* pdwProcessId
    );



/*******************************************************************
 PipeConnectionInitialize - initialize pipe connection data.

*******************************************************************/
void PipeConnectionInitialize(
    __in BURN_PIPE_CONNECTION* pConnection
    )
{
    memset(pConnection, 0, sizeof(BURN_PIPE_CONNECTION));
    pConnection->hPipe = INVALID_HANDLE_VALUE;
    pConnection->hCachePipe = INVALID_HANDLE_VALUE;
    pConnection->hLoggingPipe = INVALID_HANDLE_VALUE;
}

/*******************************************************************
 PipeConnectionUninitialize - free data in a pipe connection.

*******************************************************************/
void PipeConnectionUninitialize(
    __in BURN_PIPE_CONNECTION* pConnection
    )
{
    ReleaseFileHandle(pConnection->hLoggingPipe);
    ReleaseFileHandle(pConnection->hCachePipe);
    ReleaseFileHandle(pConnection->hPipe);
    ReleaseHandle(pConnection->hQuitRequested);
    ReleaseHandle(pConnection->hQuitMonitorThread);
    ReleaseHandle(pConnection->hProcess);
    ReleaseStr(pConnection->sczSecret);
    ReleaseStr(pConnection->sczName);

    PipeConnectionInitialize(pConnection);
}

/*******************************************************************
 PipeSendMessage - 

*******************************************************************/
extern "C" HRESULT PipeSendMessage(
    __in HANDLE hPipe,
    __in DWORD dwMessage,
    __in_bcount_opt(cbData) LPVOID pvData,
    __in SIZE_T cbData,
    __in_opt PFN_PIPE_MESSAGE_CALLBACK pfnCallback,
    __in_opt LPVOID pvContext,
    __out DWORD* pdwResult
    )
{
    HRESULT hr = S_OK;
    BURN_PIPE_RESULT result = { };

    hr = WritePipeMessage(hPipe, dwMessage, pvData, cbData);
    ExitOnFailure(hr, "Failed to write send message to pipe.");

    hr = PipePumpMessages(hPipe, pfnCallback, pvContext, &result);
    ExitOnFailure(hr, "Failed to pump messages during send message to pipe.");

    *pdwResult = result.dwResult;

LExit:
    return hr;
}

/*******************************************************************
 PipePumpMessages - 

*******************************************************************/
extern "C" HRESULT PipePumpMessages(
    __in HANDLE hPipe,
    __in_opt PFN_PIPE_MESSAGE_CALLBACK pfnCallback,
    __in_opt LPVOID pvContext,
    __in BURN_PIPE_RESULT* pResult
    )
{
    HRESULT hr = S_OK;
    BURN_PIPE_MESSAGE msg = { };
    SIZE_T iData = 0;
    LPSTR sczMessage = NULL;
    DWORD dwResult = 0;

    // Pump messages from child process.
    while (S_OK == (hr = GetPipeMessage(hPipe, &msg)))
    {
        switch (msg.dwMessage)
        {
        case BURN_PIPE_MESSAGE_TYPE_LOG:
            iData = 0;

            hr = BuffReadStringAnsi((BYTE*)msg.pvData, msg.cbData, &iData, &sczMessage);
            ExitOnFailure(hr, "Failed to read log message.");

            hr = LogStringWorkRaw(sczMessage);
            ExitOnFailure(hr, "Failed to write log message:'%hs'.", sczMessage);

            dwResult = static_cast<DWORD>(hr);
            break;

        case BURN_PIPE_MESSAGE_TYPE_COMPLETE:
            if (!msg.pvData || sizeof(DWORD) != msg.cbData)
            {
                hr = E_INVALIDARG;
                ExitOnRootFailure(hr, "No status returned to PipePumpMessages()");
            }

            pResult->dwResult = *static_cast<DWORD*>(msg.pvData);
            ExitFunction1(hr = S_OK); // exit loop.

        case BURN_PIPE_MESSAGE_TYPE_TERMINATE:
            iData = 0;

            hr = BuffReadNumber(static_cast<BYTE*>(msg.pvData), msg.cbData, &iData, &pResult->dwResult);
            ExitOnFailure(hr, "Failed to read returned result to PipePumpMessages()");

            if (sizeof(DWORD) * 2 == msg.cbData)
            {
                hr = BuffReadNumber(static_cast<BYTE*>(msg.pvData), msg.cbData, &iData, (DWORD*)&pResult->fRestart);
                ExitOnFailure(hr, "Failed to read returned restart to PipePumpMessages()");
            }

            ExitFunction1(hr = S_OK); // exit loop.

        default:
            if (pfnCallback)
            {
                hr = pfnCallback(&msg, pvContext, &dwResult);
            }
            else
            {
                hr = E_INVALIDARG;
            }
            ExitOnFailure(hr, "Failed to process message: %u", msg.dwMessage);
            break;
        }

        // post result
        hr = WritePipeMessage(hPipe, static_cast<DWORD>(BURN_PIPE_MESSAGE_TYPE_COMPLETE), &dwResult, sizeof(dwResult));
        ExitOnFailure(hr, "Failed to post result to child process.");

        FreePipeMessage(&msg);
    }
    ExitOnFailure(hr, "Failed to get message over pipe");

    if (S_FALSE == hr)
    {
        hr = S_OK;
    }

LExit:
    ReleaseStr(sczMessage);
    FreePipeMessage(&msg);

    return hr;
}

/*******************************************************************
 PipeCreateNameAndSecret - 

*******************************************************************/
extern "C" HRESULT PipeCreateNameAndSecret(
    __out_z LPWSTR *psczConnectionName,
    __out_z LPWSTR *psczSecret
    )
{
    HRESULT hr = S_OK;
    WCHAR wzGuid[GUID_STRING_LENGTH];
    LPWSTR sczConnectionName = NULL;
    LPWSTR sczSecret = NULL;

    // Create the unique pipe name.
    hr = GuidFixedCreate(wzGuid);
    ExitOnRootFailure(hr, "Failed to create pipe guid.");

    hr = StrAllocFormatted(&sczConnectionName, L"BurnPipe.%s", wzGuid);
    ExitOnFailure(hr, "Failed to allocate pipe name.");

    // Create the unique client secret.
    hr = GuidFixedCreate(wzGuid);
    ExitOnRootFailure(hr, "Failed to create pipe secret.");

    hr = StrAllocString(&sczSecret, wzGuid, 0);
    ExitOnFailure(hr, "Failed to allocate pipe secret.");

    *psczConnectionName = sczConnectionName;
    sczConnectionName = NULL;
    *psczSecret = sczSecret;
    sczSecret = NULL;

LExit:
    ReleaseStr(sczSecret);
    ReleaseStr(sczConnectionName);

    return hr;
}

/*******************************************************************
 PipeCreatePipes - create the pipes and event to signal child process.

*******************************************************************/
extern "C" HRESULT PipeCreatePipes(
    __in BURN_PIPE_CONNECTION* pConnection,
    __in BOOL fCompanion
    )
{
    Assert(pConnection->sczName);
    Assert(INVALID_HANDLE_VALUE == pConnection->hPipe);
    Assert(INVALID_HANDLE_VALUE == pConnection->hCachePipe);
    Assert(INVALID_HANDLE_VALUE == pConnection->hLoggingPipe);

    HRESULT hr = S_OK;
    PSECURITY_DESCRIPTOR psd = NULL;
    SECURITY_ATTRIBUTES sa = { };
    LPWSTR sczFullPipeName = NULL;
    HANDLE hPipe = INVALID_HANDLE_VALUE;
    HANDLE hCachePipe = INVALID_HANDLE_VALUE;
    HANDLE hLoggingPipe = INVALID_HANDLE_VALUE;

    // Only grant special rights when the pipe is being used for "embedded" scenarios.
    if (!fCompanion)
    {
        // Create the security descriptor that grants read/write/sync access to Everyone.
        // TODO: consider locking down "WD" to LogonIds (logon session)
        LPCWSTR wzSddl = L"D:(A;;GA;;;SY)(A;;GA;;;BA)(A;;GRGW0x00100000;;;WD)";
        if (!::ConvertStringSecurityDescriptorToSecurityDescriptorW(wzSddl, SDDL_REVISION_1, &psd, NULL))
        {
            ExitWithLastError(hr, "Failed to create the security descriptor for the connection event and pipe.");
        }

        sa.nLength = sizeof(sa);
        sa.lpSecurityDescriptor = psd;
        sa.bInheritHandle = FALSE;
    }

    // Create the pipe.
    hr = StrAllocFormatted(&sczFullPipeName, PIPE_NAME_FORMAT_STRING, pConnection->sczName);
    ExitOnFailure(hr, "Failed to allocate full name of pipe: %ls", pConnection->sczName);

    // TODO: consider using overlapped IO to do waits on the pipe and still be able to cancel and such.
    hPipe = ::CreateNamedPipeW(sczFullPipeName, PIPE_ACCESS_DUPLEX | FILE_FLAG_FIRST_PIPE_INSTANCE, PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT, 1, PIPE_64KB, PIPE_64KB, 1, psd ? &sa : NULL);
    if (INVALID_HANDLE_VALUE == hPipe)
    {
        ExitWithLastError(hr, "Failed to create pipe: %ls", sczFullPipeName);
    }

    if (fCompanion)
    {
        // Create the cache pipe.
        hr = StrAllocFormatted(&sczFullPipeName, CACHE_PIPE_NAME_FORMAT_STRING, pConnection->sczName);
        ExitOnFailure(hr, "Failed to allocate full name of cache pipe: %ls", pConnection->sczName);

        hCachePipe = ::CreateNamedPipeW(sczFullPipeName, PIPE_ACCESS_DUPLEX | FILE_FLAG_FIRST_PIPE_INSTANCE, PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT, 1, PIPE_64KB, PIPE_64KB, 1, NULL);
        if (INVALID_HANDLE_VALUE == hCachePipe)
        {
            ExitWithLastError(hr, "Failed to create cache pipe: %ls", sczFullPipeName);
        }

        // Create the logging pipe.
        hr = StrAllocFormatted(&sczFullPipeName, LOGGING_PIPE_NAME_FORMAT_STRING, pConnection->sczName);
        ExitOnFailure(hr, "Failed to allocate full name of logging pipe: %ls", pConnection->sczName);

        hLoggingPipe = ::CreateNamedPipeW(sczFullPipeName, PIPE_ACCESS_DUPLEX | FILE_FLAG_FIRST_PIPE_INSTANCE, PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT, 1, PIPE_64KB, PIPE_64KB, 1, NULL);
        if (INVALID_HANDLE_VALUE == hLoggingPipe)
        {
            ExitWithLastError(hr, "Failed to create logging pipe: %ls", sczFullPipeName);
        }
    }

    pConnection->hLoggingPipe = hLoggingPipe;
    hLoggingPipe = INVALID_HANDLE_VALUE;

    pConnection->hCachePipe = hCachePipe;
    hCachePipe = INVALID_HANDLE_VALUE;

    pConnection->hPipe = hPipe;
    hPipe = INVALID_HANDLE_VALUE;

LExit:
    ReleaseFileHandle(hLoggingPipe);
    ReleaseFileHandle(hCachePipe);
    ReleaseFileHandle(hPipe);
    ReleaseStr(sczFullPipeName);

    if (psd)
    {
        ::LocalFree(psd);
    }

    return hr;
}

/*******************************************************************
 PipeWaitForChildConnect - 

*******************************************************************/
extern "C" HRESULT PipeWaitForChildConnect(
    __in BURN_PIPE_CONNECTION* pConnection
    )
{
    HRESULT hr = S_OK;
    HANDLE hPipes[3] = { pConnection->hPipe, pConnection->hCachePipe, pConnection->hLoggingPipe};
    LPCWSTR wzSecret = pConnection->sczSecret;
    DWORD cbSecret = lstrlenW(wzSecret) * sizeof(WCHAR);
    DWORD dwCurrentProcessId = ::GetCurrentProcessId();
    DWORD dwAck = 0;

    for (DWORD i = 0; i < countof(hPipes) && INVALID_HANDLE_VALUE != hPipes[i]; ++i)
    {
        HANDLE hPipe = hPipes[i];
        DWORD dwPipeState = PIPE_READMODE_BYTE | PIPE_NOWAIT;

        // Temporarily make the pipe non-blocking so we will not get stuck in ::ConnectNamedPipe() forever
        // if the child decides not to show up.
        if (!::SetNamedPipeHandleState(hPipe, &dwPipeState, NULL, NULL))
        {
            ExitWithLastError(hr, "Failed to set pipe to non-blocking.");
        }

        // Loop for a while waiting for a connection from child process.
        DWORD cRetry = 0;
        do
        {
            if (!::ConnectNamedPipe(hPipe, NULL))
            {
                DWORD er = ::GetLastError();
                if (ERROR_PIPE_CONNECTED == er)
                {
                    hr = S_OK;
                    break;
                }
                else if (ERROR_PIPE_LISTENING == er)
                {
                    if (cRetry < PIPE_RETRY_FOR_CONNECTION)
                    {
                        hr = HRESULT_FROM_WIN32(er);
                    }
                    else
                    {
                        hr = HRESULT_FROM_WIN32(ERROR_TIMEOUT);
                        break;
                    }

                    ++cRetry;
                    ::Sleep(PIPE_WAIT_FOR_CONNECTION);
                }
                else
                {
                    hr = HRESULT_FROM_WIN32(er);
                    break;
                }
            }
        } while (HRESULT_FROM_WIN32(ERROR_PIPE_LISTENING) == hr);
        ExitOnRootFailure(hr, "Failed to wait for child to connect to pipe.");

        // Put the pipe back in blocking mode.
        dwPipeState = PIPE_READMODE_BYTE | PIPE_WAIT;
        if (!::SetNamedPipeHandleState(hPipe, &dwPipeState, NULL, NULL))
        {
            ExitWithLastError(hr, "Failed to reset pipe to blocking.");
        }

        // Prove we are the one that created the elevated process by passing the secret.
        hr = FileWriteHandle(hPipe, reinterpret_cast<LPCBYTE>(&cbSecret), sizeof(cbSecret));
        ExitOnFailure(hr, "Failed to write secret length to pipe.");

        hr = FileWriteHandle(hPipe, reinterpret_cast<LPCBYTE>(wzSecret), cbSecret);
        ExitOnFailure(hr, "Failed to write secret to pipe.");

        hr = FileWriteHandle(hPipe, reinterpret_cast<LPCBYTE>(&dwCurrentProcessId), sizeof(dwCurrentProcessId));
        ExitOnFailure(hr, "Failed to write our process id to pipe.");

        // Wait until the elevated process responds that it is ready to go.
        hr = FileReadHandle(hPipe, reinterpret_cast<LPBYTE>(&dwAck), sizeof(dwAck));
        ExitOnFailure(hr, "Failed to read ACK from pipe.");

        // The ACK should match out expected child process id.
        //if (pConnection->dwProcessId != dwAck)
        //{
        //    hr = HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
        //    ExitOnRootFailure(hr, "Incorrect ACK from elevated pipe: %u", dwAck);
        //}
    }

LExit:
    return hr;
}

/*******************************************************************
 PipeTerminateLoggingPipe - 

*******************************************************************/
extern "C" HRESULT PipeTerminateLoggingPipe(
    __in HANDLE hLoggingPipe,
    __in DWORD dwParentExitCode
    )
{
    HRESULT hr = S_OK;
    BYTE* pbData = NULL;
    SIZE_T cbData = 0;

    // Prepare the exit message.
    hr = BuffWriteNumber(&pbData, &cbData, dwParentExitCode);
    ExitOnFailure(hr, "Failed to write exit code to message buffer.");

    hr = WritePipeMessage(hLoggingPipe, static_cast<DWORD>(BURN_PIPE_MESSAGE_TYPE_COMPLETE), pbData, cbData);
    ExitOnFailure(hr, "Failed to post complete message to logging pipe.");

LExit:
    ReleaseBuffer(pbData);

    return hr;
}

/*******************************************************************
 PipeTerminateChildProcess - 

*******************************************************************/
extern "C" HRESULT PipeTerminateChildProcess(
    __in BURN_PIPE_CONNECTION* pConnection,
    __in DWORD dwParentExitCode,
    __in BOOL fRestart
    )
{
    HRESULT hr = S_OK;
    BYTE* pbData = NULL;
    SIZE_T cbData = 0;
    BOOL fTimedOut = FALSE;

    if (pConnection->hQuitRequested)
    {
        ::SetEvent(pConnection->hQuitRequested);
    }

    // Prepare the exit message.
    hr = BuffWriteNumber(&pbData, &cbData, dwParentExitCode);
    ExitOnFailure(hr, "Failed to write exit code to message buffer.");

    hr = BuffWriteNumber(&pbData, &cbData, fRestart);
    ExitOnFailure(hr, "Failed to write restart to message buffer.");

    // Send the messages.
    if (INVALID_HANDLE_VALUE != pConnection->hCachePipe)
    {
        hr = WritePipeMessage(pConnection->hCachePipe, static_cast<DWORD>(BURN_PIPE_MESSAGE_TYPE_TERMINATE), pbData, cbData);
        ExitOnFailure(hr, "Failed to post terminate message to child process cache thread.");
    }

    hr = WritePipeMessage(pConnection->hPipe, static_cast<DWORD>(BURN_PIPE_MESSAGE_TYPE_TERMINATE), pbData, cbData);
    ExitOnFailure(hr, "Failed to post terminate message to child process.");

    // If we were able to get a handle to the other process, wait for it to exit.
    if (pConnection->hProcess)
    {
        hr = AppWaitForSingleObject(pConnection->hProcess, PIPE_WAIT_FOR_CONNECTION * PIPE_RETRY_FOR_CONNECTION);
        ExitOnWaitObjectFailure(hr, fTimedOut, "Failed to wait for child process exit.");

        AssertSz(!fTimedOut, "Timed out while waiting for child process to exit.");
    }

#ifdef DEBUG
    if (pConnection->hProcess && !fTimedOut)
    {
        DWORD dwChildExitCode = 0;
        HRESULT hrDebug = S_OK;

        hrDebug = CoreWaitForProcCompletion(pConnection->hProcess, 0, &dwChildExitCode);
        if (E_ACCESSDENIED != hrDebug && FAILED(hrDebug)) // if the other process is elevated and we are not, then we'll get ERROR_ACCESS_DENIED.
        {
            TraceError(hrDebug, "Failed to wait for child process completion.");
        }

        AssertSz(E_ACCESSDENIED == hrDebug || dwChildExitCode == dwParentExitCode,
                 "Child elevated process did not return matching exit code to parent process.");
    }
#endif

LExit:
    ReleaseBuffer(pbData);

    return hr;
}

/*******************************************************************
 PipeChildConnect - Called from the child process to connect back
                    to the pipe provided by the parent process.

*******************************************************************/
extern "C" HRESULT PipeChildConnect(
    __in BURN_PIPE_CONNECTION* pConnection,
    __in BOOL fCompanion
    )
{
    Assert(pConnection->sczName);
    Assert(pConnection->sczSecret);
    Assert(INVALID_HANDLE_VALUE == pConnection->hPipe);
    Assert(INVALID_HANDLE_VALUE == pConnection->hCachePipe);
    Assert(INVALID_HANDLE_VALUE == pConnection->hLoggingPipe);

    HRESULT hr = S_OK;
    LPWSTR sczPipeName = NULL;

    // Try to connect to the parent.
    hr = StrAllocFormatted(&sczPipeName, PIPE_NAME_FORMAT_STRING, pConnection->sczName);
    ExitOnFailure(hr, "Failed to allocate name of parent pipe.");

    hr = E_UNEXPECTED;
    for (DWORD cRetry = 0; FAILED(hr) && cRetry < PIPE_RETRY_FOR_CONNECTION; ++cRetry)
    {
        pConnection->hPipe = ::CreateFileW(sczPipeName, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
        if (INVALID_HANDLE_VALUE == pConnection->hPipe)
        {
            hr = HRESULT_FROM_WIN32(::GetLastError());
            if (E_FILENOTFOUND == hr) // if the pipe isn't created, call it a timeout waiting on the parent.
            {
                hr = HRESULT_FROM_WIN32(ERROR_TIMEOUT);
            }

            ::Sleep(PIPE_WAIT_FOR_CONNECTION);
        }
        else // we have a connection, go with it.
        {
            hr = S_OK;
        }
    }
    ExitOnRootFailure(hr, "Failed to open parent pipe: %ls", sczPipeName)

    // Verify the parent and notify it that the child connected.
    hr = ChildPipeConnected(pConnection->hPipe, pConnection->sczSecret, &pConnection->dwProcessId);
    ExitOnFailure(hr, "Failed to verify parent pipe: %ls", sczPipeName);

    if (fCompanion)
    {
        // Connect to the parent for the cache pipe.
        hr = StrAllocFormatted(&sczPipeName, CACHE_PIPE_NAME_FORMAT_STRING, pConnection->sczName);
        ExitOnFailure(hr, "Failed to allocate name of parent cache pipe.");

        pConnection->hCachePipe = ::CreateFileW(sczPipeName, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
        if (INVALID_HANDLE_VALUE == pConnection->hCachePipe)
        {
            ExitWithLastError(hr, "Failed to open parent cache pipe: %ls", sczPipeName)
        }

        // Verify the parent and notify it that the child connected.
        hr = ChildPipeConnected(pConnection->hCachePipe, pConnection->sczSecret, &pConnection->dwProcessId);
        ExitOnFailure(hr, "Failed to verify parent cache pipe: %ls", sczPipeName);

        // Connect to the parent for the logging pipe.
        hr = StrAllocFormatted(&sczPipeName, LOGGING_PIPE_NAME_FORMAT_STRING, pConnection->sczName);
        ExitOnFailure(hr, "Failed to allocate name of parent logging pipe.");

        pConnection->hLoggingPipe = ::CreateFileW(sczPipeName, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
        if (INVALID_HANDLE_VALUE == pConnection->hLoggingPipe)
        {
            ExitWithLastError(hr, "Failed to open parent logging pipe: %ls", sczPipeName)
        }

        // Verify the parent and notify it that the child connected.
        hr = ChildPipeConnected(pConnection->hLoggingPipe, pConnection->sczSecret, &pConnection->dwProcessId);
        ExitOnFailure(hr, "Failed to verify parent logging pipe: %ls", sczPipeName);
    }

    if (!pConnection->hProcess)
    {
        pConnection->hProcess = ::OpenProcess(SYNCHRONIZE, FALSE, pConnection->dwProcessId);
        ExitOnNullWithLastError(pConnection->hProcess, hr, "Failed to open companion process with PID: %u", pConnection->dwProcessId);
    }

LExit:
    ReleaseStr(sczPipeName);

    return hr;
}


static HRESULT AllocatePipeMessage(
    __in DWORD dwMessage,
    __in_bcount_opt(cbData) LPVOID pvData,
    __in SIZE_T cbData,
    __out_bcount(cb) LPVOID* ppvMessage,
    __out SIZE_T* pcbMessage
    )
{
    HRESULT hr = S_OK;
    LPVOID pv = NULL;
    size_t cb = 0;
    DWORD dwcbData = 0;

    // If no data was provided, ensure the count of bytes is zero.
    if (!pvData)
    {
        cbData = 0;
    }
    else if (MAXDWORD < cbData)
    {
        ExitWithRootFailure(hr, E_INVALIDDATA, "Pipe message is too large.");
    }

    hr = ::SizeTAdd(sizeof(dwMessage) + sizeof(dwcbData), cbData, &cb);
    ExitOnRootFailure(hr, "Failed to calculate total pipe message size");

    dwcbData = (DWORD)cbData;

    // Allocate the message.
    pv = MemAlloc(cb, FALSE);
    ExitOnNull(pv, hr, E_OUTOFMEMORY, "Failed to allocate memory for message.");

    memcpy_s(pv, cb, &dwMessage, sizeof(dwMessage));
    memcpy_s(static_cast<BYTE*>(pv) + sizeof(dwMessage), cb - sizeof(dwMessage), &dwcbData, sizeof(dwcbData));
    if (dwcbData)
    {
        memcpy_s(static_cast<BYTE*>(pv) + sizeof(dwMessage) + sizeof(dwcbData), cb - sizeof(dwMessage) - sizeof(dwcbData), pvData, dwcbData);
    }

    *pcbMessage = cb;
    *ppvMessage = pv;
    pv = NULL;

LExit:
    ReleaseMem(pv);
    return hr;
}

static void FreePipeMessage(
    __in BURN_PIPE_MESSAGE *pMsg
    )
{
    if (pMsg->fAllocatedData)
    {
        ReleaseNullMem(pMsg->pvData);
        pMsg->fAllocatedData = FALSE;
    }
}

static HRESULT WritePipeMessage(
    __in HANDLE hPipe,
    __in DWORD dwMessage,
    __in_bcount_opt(cbData) LPVOID pvData,
    __in SIZE_T cbData
    )
{
    HRESULT hr = S_OK;
    LPVOID pv = NULL;
    SIZE_T cb = 0;

    hr = AllocatePipeMessage(dwMessage, pvData, cbData, &pv, &cb);
    ExitOnFailure(hr, "Failed to allocate message to write.");

    // Write the message.
    hr = FileWriteHandle(hPipe, reinterpret_cast<LPCBYTE>(pv), cb);
    ExitOnFailure(hr, "Failed to write message type to pipe.");

LExit:
    ReleaseMem(pv);
    return hr;
}

static HRESULT GetPipeMessage(
    __in HANDLE hPipe,
    __in BURN_PIPE_MESSAGE* pMsg
    )
{
    HRESULT hr = S_OK;
    BYTE pbMessageAndByteCount[sizeof(DWORD) + sizeof(DWORD)] = { };

    hr = FileReadHandle(hPipe, pbMessageAndByteCount, sizeof(pbMessageAndByteCount));
    if (HRESULT_FROM_WIN32(ERROR_BROKEN_PIPE) == hr)
    {
        memset(pbMessageAndByteCount, 0, sizeof(pbMessageAndByteCount));
        hr = S_FALSE;
    }
    ExitOnFailure(hr, "Failed to read message from pipe.");

    pMsg->dwMessage = *(DWORD*)(pbMessageAndByteCount);
    pMsg->cbData = *(DWORD*)(pbMessageAndByteCount + sizeof(DWORD));
    if (pMsg->cbData)
    {
        pMsg->pvData = MemAlloc(pMsg->cbData, FALSE);
        ExitOnNull(pMsg->pvData, hr, E_OUTOFMEMORY, "Failed to allocate data for message.");

        hr = FileReadHandle(hPipe, reinterpret_cast<LPBYTE>(pMsg->pvData), pMsg->cbData);
        ExitOnFailure(hr, "Failed to read data for message.");

        pMsg->fAllocatedData = TRUE;
    }

LExit:
    if (!pMsg->fAllocatedData && pMsg->pvData)
    {
        MemFree(pMsg->pvData);
    }

    return hr;
}

static HRESULT ChildPipeConnected(
    __in HANDLE hPipe,
    __in_z LPCWSTR wzSecret,
    __inout DWORD* pdwProcessId
    )
{
    HRESULT hr = S_OK;
    LPWSTR sczVerificationSecret = NULL;
    DWORD cbVerificationSecret = 0;
    DWORD dwVerificationProcessId = 0;
    DWORD dwAck = ::GetCurrentProcessId(); // send our process id as the ACK.

    // Read the verification secret.
    hr = FileReadHandle(hPipe, reinterpret_cast<LPBYTE>(&cbVerificationSecret), sizeof(cbVerificationSecret));
    ExitOnFailure(hr, "Failed to read size of verification secret from parent pipe.");

    if (255 < cbVerificationSecret / sizeof(WCHAR))
    {
        hr = HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
        ExitOnRootFailure(hr, "Verification secret from parent is too big.");
    }

    hr = StrAlloc(&sczVerificationSecret, cbVerificationSecret / sizeof(WCHAR) + 1);
    ExitOnFailure(hr, "Failed to allocate buffer for verification secret.");

    FileReadHandle(hPipe, reinterpret_cast<LPBYTE>(sczVerificationSecret), cbVerificationSecret);
    ExitOnFailure(hr, "Failed to read verification secret from parent pipe.");

    // Verify the secrets match.
    if (CSTR_EQUAL != ::CompareStringW(LOCALE_NEUTRAL, 0, sczVerificationSecret, -1, wzSecret, -1))
    {
        hr = HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
        ExitOnRootFailure(hr, "Verification secret from parent does not match.");
    }

    // Read the verification process id.
    hr = FileReadHandle(hPipe, reinterpret_cast<LPBYTE>(&dwVerificationProcessId), sizeof(dwVerificationProcessId));
    ExitOnFailure(hr, "Failed to read verification process id from parent pipe.");

    // If a process id was not provided, we'll trust the process id from the parent.
    if (*pdwProcessId == 0)
    {
        *pdwProcessId = dwVerificationProcessId;
    }
    else if (*pdwProcessId != dwVerificationProcessId) // verify the ids match.
    {
        hr = HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
        ExitOnRootFailure(hr, "Verification process id from parent does not match.");
    }

    // All is well, tell the parent process.
    // TODO: consider sending BURN_PROTOCOL_VERSION as a way to verify compatibility.
    hr = FileWriteHandle(hPipe, reinterpret_cast<LPCBYTE>(&dwAck), sizeof(dwAck));
    ExitOnFailure(hr, "Failed to inform parent process that child is running.");

LExit:
    ReleaseStr(sczVerificationSecret);
    return hr;
}
