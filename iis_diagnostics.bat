@echo off
echo =========================================================
echo IIS ASP.NET Core Diagnostics Helper
echo =========================================================
echo Running diagnostics to help fix 502.5 ANCM errors...

echo.
echo Checking for ASP.NET Core Module installation...
dism /online /get-packages | findstr "Microsoft-Windows-IIS-AspNetCore"
if %errorlevel% neq 0 (
    echo WARNING: ASP.NET Core Module might not be installed!
    echo Install the ASP.NET Core Hosting Bundle from:
    echo https://dotnet.microsoft.com/download/dotnet/9.0
)

echo.
echo Checking .NET Runtime versions...
dotnet --list-runtimes | findstr "Microsoft.AspNetCore.App"

echo.
echo Checking IIS Module Registration...
%windir%\system32\inetsrv\appcmd list modules /name:AspNetCoreModuleV2
if %errorlevel% neq 0 (
    echo WARNING: AspNetCoreModuleV2 is not registered in IIS!
    echo Install the ASP.NET Core Hosting Bundle and run 'iisreset'
)

echo.
echo Checking published application files...
for %%p in (WebSpark.Web WebSpark.Portal DataSpark.Web) do (
    echo Checking %%p...
    if exist "C:\PublishedWebsites\%%p\%%p.dll" (
        echo - %%p.dll found
    ) else (
        echo ERROR: %%p.dll not found!
    )
    
    if exist "C:\PublishedWebsites\%%p\web.config" (
        echo - web.config found
        findstr "aspNetCore" "C:\PublishedWebsites\%%p\web.config" > nul
        if %errorlevel% neq 0 (
            echo ERROR: web.config does not contain aspNetCore configuration!
        )
    ) else (
        echo ERROR: web.config not found!
    )
    
    echo - Checking for appsettings.json
    if exist "C:\PublishedWebsites\%%p\appsettings.json" (
        echo - appsettings.json found
    ) else (
        echo WARNING: appsettings.json not found! Application might be missing configuration.
    )
    
    echo.
)

echo.
echo Checking IIS App Pool Configuration...
%windir%\system32\inetsrv\appcmd list apppool
echo.
echo RECOMMENDATION: Ensure app pools are configured with "No Managed Code"
echo.

echo =========================================================
echo Manual verification steps:
echo 1. Check log files in each app's 'logs' folder
echo 2. Try running test_app.bat in each app folder to test standalone execution
echo 3. Verify application pool identity has permissions to the application folders
echo 4. Check for missing dependencies in the published folders
echo 5. Ensure connection strings and configuration are correct
echo =========================================================

pause
