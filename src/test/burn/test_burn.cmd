@setlocal
@pushd %~dp0

@set _RESULT=0
@set _C=Debug
@set _L=%~dp0..\..\..\build\logs
:parse_args
@if /i "%1"=="release" set _C=Release
@if /i "%1"=="inc" set _INC=1
@if /i "%1"=="clean" set _CLEAN=1
@if /i "%1"=="test" set RuntimeTestsEnabled=true
@if not "%1"=="" shift & goto parse_args

@set _B=%~dp0..\..\..\build\IntegrationBurn\%_C%

:: Clean

@if "%_INC%"=="" call :clean
@if NOT "%_CLEAN%"=="" goto :end

@echo Burn integration tests %_C%

msbuild -Restore test_burn_t.proj -p:Configuration=%_C% -tl -nologo -m -warnaserror -bl:%_L%\test_burn_build.binlog || exit /b
msbuild -Restore TestData\TestData.proj -p:Configuration=%_C% -tl -nologo -m -warnaserror -bl:%_L%\test_burn_data_build.binlog || exit /b
msbuild -Restore TestData6\TestData6.proj -p:Configuration=%_C% -tl -nologo -m -warnaserror -bl:%_L%\test_burn_data6_build.binlog || exit /b

"%_B%\net462\win-x86\testexe.exe" /dm "%_B%\net8.0-windows\testhost.exe"
mt.exe -manifest "WixToolsetTest.BurnE2E\testhost.longpathaware.manifest" -updateresource:"%_B%\net8.0-windows\testhost.exe"

@if not "%RuntimeTestsEnabled%"=="true" goto :LExit

dotnet test -c %_C% WixToolsetTest.BurnE2E --nologo --no-build -l "trx;LogFileName=%_L%\TestResults\WixToolsetTest.BurnE2E.trx" || exit /b
dotnet test ..\..\..\build\BurnUnitTest\%_C%\net8.0-windows\WixToolsetTest.BurnUnitTest.dll --settings ..\..\..\build\BurnUnitTest\%_C%\net8.0-windows\BurnBaTests.runsettings --nologo --no-build -l "trx;LogFileName=%_L%\TestResults\WixToolsetTest.BurnUnitTests.trx" || exit /b

@goto :LExit

:clean
@rd /s/q "..\..\build\test" 2> nul
@del "..\..\build\artifacts\PanelSwWix4.TestTools.*.nupkg" 2> nul
@rd /s/q "%USERPROFILE%\.nuget\packages\PanelSwWix4.TestTools" 2> nul
@exit /b

:LExit
@popd
@endlocal
