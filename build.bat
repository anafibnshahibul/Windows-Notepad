@echo off
setlocal enabledelayedexpansion

echo ========================================
echo   Advanced Notepad - Windows Build Script
echo   (auto-installs missing prerequisites)
echo ========================================
echo.

REM ---- Must run elevated for installs ----
net session >nul 2>nul
if errorlevel 1 (
    echo This script needs to install software and must run as Administrator.
    echo Right-click build.bat and choose "Run as administrator".
    pause
    exit /b 1
)

set CONFIG=Release
set PLATFORM=x64
set SOLUTION=AdvancedNotepad.sln
set WIX_PROJECT=Product.wxs
set OUTPUT_MSI=AdvancedNotepad.msi

REM ============================================================
REM Step 0: Ensure winget is available (comes with Win11 by default)
REM ============================================================
where winget >nul 2>nul
if errorlevel 1 (
    echo winget not found. It ships with Windows 11 by default.
    echo Install "App Installer" from the Microsoft Store, then re-run this script.
    goto :error
)

REM ============================================================
REM Step 1: .NET 8 SDK
REM ============================================================
echo [1/6] Checking .NET 8 SDK...
where dotnet >nul 2>nul
if errorlevel 1 (
    goto :install_dotnet
)
dotnet --list-sdks | findstr /r "^8\." >nul 2>nul
if errorlevel 1 (
    goto :install_dotnet
)
echo   .NET 8 SDK already installed.
goto :dotnet_done

:install_dotnet
echo   .NET 8 SDK not found. Installing via winget...
winget install --id Microsoft.DotNet.SDK.8 -e --accept-source-agreements --accept-package-agreements
if errorlevel 1 (
    echo ERROR: Failed to install .NET 8 SDK automatically.
    echo        Install manually from https://dotnet.microsoft.com/download/dotnet/8.0
    goto :error
)
REM refresh PATH for current session
call :refresh_path

:dotnet_done
echo.

REM ============================================================
REM Step 2: Visual Studio Build Tools (MSBuild + C++ workload)
REM ============================================================
echo [2/6] Checking Visual Studio Build Tools / MSBuild...
set VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

if not exist %VSWHERE% (
    goto :install_vs
)

for /f "usebackq tokens=*" %%i in (`%VSWHERE% -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
    set MSBUILD_PATH=%%i
)

if not defined MSBUILD_PATH (
    goto :install_vs
)
echo   Found MSBuild: %MSBUILD_PATH%
goto :vs_done

:install_vs
echo   Visual Studio Build Tools with C++ workload not found. Installing via winget...
echo   (This step can take 10-20 minutes on first run.)
winget install --id Microsoft.VisualStudio.2022.BuildTools -e --accept-source-agreements --accept-package-agreements ^
    --override "--wait --quiet --add Microsoft.VisualStudio.Workload.VCTools --add Microsoft.VisualStudio.Workload.MSBuildTools --add Microsoft.Component.MSBuild --includeRecommended"
if errorlevel 1 (
    echo ERROR: Failed to install Visual Studio Build Tools automatically.
    echo        Install manually from https://visualstudio.microsoft.com/downloads/
    echo        and select "Desktop development with C++".
    goto :error
)

for /f "usebackq tokens=*" %%i in (`%VSWHERE% -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
    set MSBUILD_PATH=%%i
)
if not defined MSBUILD_PATH (
    echo ERROR: MSBuild still not found after install. A machine restart or new
    echo        terminal session may be required. Re-run this script after restarting.
    goto :error
)

:vs_done
echo.

REM ============================================================
REM Step 3: WiX Toolset CLI (v4+)
REM ============================================================
echo [3/6] Checking WiX Toolset...
where wix >nul 2>nul
if errorlevel 1 (
    echo   WiX CLI not found. Installing as a global dotnet tool...
    dotnet tool install --global wix
    if errorlevel 1 (
        echo ERROR: Failed to install WiX toolset automatically.
        echo        Install manually with: dotnet tool install --global wix
        goto :error
    )
    call :refresh_path
) else (
    echo   WiX CLI already installed.
)

wix extension list -g | findstr /i "WixToolset.UI.wixext" >nul 2>nul
if errorlevel 1 (
    echo   Adding WiX UI extension (EULA / InstallDir dialogs)...
    wix extension add WixToolset.UI.wixext -g
)
echo.

REM ============================================================
REM Step 4: Restore packages
REM ============================================================
echo [4/6] Restoring packages...
dotnet restore "%SOLUTION%"
if errorlevel 1 (
    echo ERROR: dotnet restore failed.
    goto :error
)
echo.

REM ============================================================
REM Step 5: Build solution (WPF app + native C++ DLL)
REM ============================================================
echo [5/6] Building solution (%CONFIG%^|%PLATFORM%)...
"%MSBUILD_PATH%" "%SOLUTION%" /p:Configuration=%CONFIG% /p:Platform=%PLATFORM% /m /v:minimal
if errorlevel 1 (
    echo ERROR: Build failed. See output above.
    goto :error
)
echo   Build succeeded.
echo.

REM ============================================================
REM Step 6: Build MSI installer via WiX
REM ============================================================
echo [6/6] Building MSI installer...
wix build "%WIX_PROJECT%" -ext WixToolset.UI.wixext -arch %PLATFORM% -o "%OUTPUT_MSI%"
if errorlevel 1 (
    echo ERROR: WiX build failed. See output above.
    goto :error
)

echo.
echo ========================================
echo   BUILD SUCCEEDED
echo   Installer: %CD%\%OUTPUT_MSI%
echo ========================================
goto :end

:refresh_path
REM Re-read PATH from registry so newly installed CLIs are visible in this session
for /f "tokens=2*" %%A in ('reg query "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" /v Path') do set "SYS_PATH=%%B"
for /f "tokens=2*" %%A in ('reg query "HKCU\Environment" /v Path 2^>nul') do set "USR_PATH=%%B"
set "PATH=%SYS_PATH%;%USR_PATH%"
exit /b 0

:error
echo.
echo ========================================
echo   BUILD FAILED
echo ========================================
pause
exit /b 1

:end
endlocal
pause
