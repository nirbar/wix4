@setlocal
@pushd %~dp0

@set _C=Debug
@set _L=%~dp0..\..\build\logs

:parse_args
@if /i "%1"=="release" set _C=Release
@if /i "%1"=="inc" set _INC=1
@if /i "%1"=="clean" set _CLEAN=1
@if not "%1"=="" shift & goto parse_args

@set _B=%~dp0..\..\build\wix\%_C%

:: Clean

@if "%_INC%"=="" call :clean
@if NOT "%_CLEAN%"=="" goto :end

@echo Building test %_C%

:: test
nuget restore || exit /b

msbuild test_t.proj -p:Configuration=%_C% -tl -nologo -warnaserror -bl:%_L%\test_build.binlog || exit /b

@goto :end

:clean
@rd /s/q "..\..\build\test" 2> nul
@del "..\..\build\artifacts\PanelSwWix4.TestSupport.*.nupkg" 2> nul
@del "..\..\build\artifacts\PanelSwWix4.MSTestSupport.*.nupkg" 2> nul
@rd /s/q "%USERPROFILE%\.nuget\packages\PanelSwWix4.TestSupport" 2> nul
@rd /s/q "%USERPROFILE%\.nuget\packages\PanelSwWix4.MSTestSupport" 2> nul
@exit /b

:end
@popd
@endlocal
