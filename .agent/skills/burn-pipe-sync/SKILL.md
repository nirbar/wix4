---
name: burn-pipe-sync
description: >
  Use when adding, removing, or renaming pipe message constants in the burn engine's
  C++ headers or callback files — specifically BOOTSTRAPPER_APPLICATION_MESSAGE (BA pipe),
  BOOTSTRAPPER_ENGINE_MESSAGE (BAEngine pipe), or the unittest-private protocol constants
  in unittest.h. Ensures the C# BA unit-test framework in
  src/test/burn/WixToolset.Burn.UnitTest stays in sync with the C++ wire format.
---

# Burn Pipe Message Sync

When the C++ burn engine pipe protocol changes, the corresponding C# test-framework
files **must** be updated in lockstep.  Use this checklist whenever you touch the
files listed in the **C++ sources** column.

---

## Mapping: C++ source → C# counterpart

| What changed | C++ source of truth | C# file(s) to update |
|---|---|---|
| Added/removed/renamed `BOOTSTRAPPER_APPLICATION_MESSAGE_*` value | `src/api/burn/inc/BootstrapperApplicationTypes.h` — `BOOTSTRAPPER_APPLICATION_MESSAGE` enum | `src/test/burn/WixToolset.Burn.UnitTest/Internal/BurnBAMessageDispatcher.cs` — `BurnApplicationMessage` enum AND `DispatchCore` switch |
| Changed serialized field order/types for an `OnXxx` BA callback | `src/burn/engine/bacallback.cpp` — `BuffWriteNumberToBuffer` / `BuffWriteStringToBuffer` call sequence for that message | `src/test/burn/WixToolset.Burn.UnitTest/Internal/BurnBAMessageDispatcher.cs` — the matching `case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONxxx` block in `DispatchCore` |
| Added/removed/renamed `BOOTSTRAPPER_ENGINE_MESSAGE_*` value | `src/api/burn/inc/BootstrapperEngineTypes.h` — `BOOTSTRAPPER_ENGINE_MESSAGE` enum | Currently the engine pipe is relayed opaquely by `RealBAPipeServer` / `RelayEngineMessages`; no enum mirror exists yet — **add one** if the new message needs selective handling in the test runner |
| Changed `UNITTEST_APPLICATION_MESSAGE_START_REAL_BA` value or the `BA_ONUNITTESTAPPLICATIONSTARTREALBA_ARGS/RESULTS` struct layout | `src/burn/engine/unittest.h` | `src/test/burn/WixToolset.Burn.UnitTest/Internal/BurnProtocolConstants.cs` — `BaMessageStartRealBA` constant and `BurnBATestRunner.HandleStartRealBAAsync` deserialization |
| Changed `UNITTEST_ENGINE_MESSAGE_QUIT` value or the `BAENGINE_UNITTESTQUIT_ARGS/RESULTS` struct layout | `src/burn/engine/unittest.h` | `src/test/burn/WixToolset.Burn.UnitTest/Internal/BurnProtocolConstants.cs` — `EngineMessageQuit` constant and `BurnBATestRunner.SendQuitAsync` serialization |
| Changed `WIX_5_BOOTSTRAPPER_APPLICATION_API_VERSION` or the BA pipe handshake format | `src/api/burn/inc/BootstrapperApplicationTypes.h` | `src/test/burn/WixToolset.Burn.UnitTest/Internal/BurnProtocolConstants.cs` — `ApiVersion` constant; also review `BurnPipeConnection.ConnectAsync` and `RealBAPipeServer.WaitForClientConnectAsync` |
| Changed `.BA` or `.BAEngine` named-pipe suffix used by burn | `src/burn/engine/pipe.cpp` (or wherever the suffix is defined) | `src/test/burn/WixToolset.Burn.UnitTest/Internal/BurnProtocolConstants.cs` — `BaPipeSuffix` / `BAEnginePipeSuffix` |

---

## Key design rules

1. **`BurnApplicationMessage` enum is a mirror**, not a reference.  The values are assigned by
   auto-increment starting at `BOOTSTRAPPER_APPLICATION_MESSAGE_UNKNOWN = 0x10000` (65536).
   The C++ header is the source of truth; the C# enum must stay identical in name and order.

2. **`DispatchCore` serialization order must exactly match `bacallback.cpp`.**
   For each message, `bacallback.cpp` writes args with a sequence of `BuffWriteNumberToBuffer` /
   `BuffWriteStringToBuffer` calls; `DispatchCore` reads them with the matching
   `a.ReadUInt32()` / `a.ReadString()` / etc. calls **in the same order**.
   The results (written by `DispatchCore`, read back by `bacallback.cpp`) must also match.

3. **Unittest-private constants in `unittest.h` are intentionally NOT part of the public API.**
   They are duplicated in `BurnProtocolConstants.cs` by design; a comment in `unittest.h`
   says so.  Keep both files in sync manually whenever `unittest.h` changes.

4. **String fields** in `bacallback.cpp` use `BuffWriteStringToBuffer` which writes
   `[uint32 charCount][UTF-16LE chars, no null terminator]`.  In C# this is
   `BurnBufferWriter.WriteString` / `BurnBufferReader.ReadString`.

5. **`HRESULT` fields and `bool` fields** have specific widths in the wire format:
   - HRESULT → `int` (4 bytes, `WriteInt32` / `ReadInt32`)
   - `BOOL` → 4-byte integer (`WriteBool` / `ReadBool`)
   - Pointer-sized values (e.g. `HWND`) → `uint64` on x64 (`WriteUInt64` / `ReadUInt64`)

---

## Checklist for a new `BOOTSTRAPPER_APPLICATION_MESSAGE_ON*` message

- [ ] Add the enum value at the **end** of `BurnApplicationMessage` in `BurnBAMessageDispatcher.cs`
      (auto-increment; must stay in the same relative position as in the C++ header).
- [ ] Add a `virtual int OnXxx(...)` method to `BurnBATestBase.cs` with a default implementation
      that calls `_messageContext!.ForwardToRealBA(); return _messageContext.ResponseHr;`.
- [ ] Add the corresponding `case BurnApplicationMessage.BOOTSTRAPPER_APPLICATION_MESSAGE_ONxxx:`
      block to `DispatchCore` in `BurnBAMessageDispatcher.cs` that:
      - Reads args in the exact order `bacallback.cpp` writes them.
      - Calls `int hr = CallDispatch(instance, () => instance.OnXxx(...));`
      - After `CallDispatch`, adds `if (instance.TestFailureException != null) { fCancel = true; }`
        when the message has a cancellable `fCancel` output parameter.
      - Writes results in the exact order `bacallback.cpp` reads them back.
- [ ] If the message has a `fCancel` output, declare `bool fCancel = false;` **before**
      `CallDispatch` so the cancel guard can set it on failure.

## Checklist for removing or renaming a `BOOTSTRAPPER_APPLICATION_MESSAGE_ON*` message

- [ ] Remove (or rename) the enum value in `BurnApplicationMessage`.
- [ ] Remove (or rename) the `virtual int OnXxx(...)` in `BurnBATestBase.cs`.
- [ ] Remove (or rename) the `case` block in `DispatchCore`.
- [ ] Search all example/test projects under `src/test/burn/` for overrides of the old method.
