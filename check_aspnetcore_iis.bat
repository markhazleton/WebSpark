@echo off
echo =========================================================
echo ASP.NET Core Module for IIS Check
echo =========================================================
echo This script checks if the ASP.NET Core Module v2 is properly installed.

echo.
echo 1. Checking if AspNetCoreModuleV2 is registered in IIS...
%windir%\system32\inetsrv\appcmd list modules /name:AspNetCoreModuleV2
if %errorlevel% neq 0 (
    echo.
    echo ERROR: AspNetCoreModuleV2 is not registered in IIS!
    echo.
    echo You need to install the ASP.NET Core Hosting Bundle from:
    echo https://dotnet.microsoft.com/download/dotnet/9.0
    echo.
    echo After installation, run 'iisreset' from an administrator command prompt.
) else (
    echo.
    echo SUCCESS: AspNetCoreModuleV2 is properly registered in IIS.
)

echo.
echo 2. Checking for .NET Core Runtime installation...
dotnet --list-runtimes | findstr "Microsoft.AspNetCore.App 9."
if %errorlevel% neq 0 (
    echo.
    echo WARNING: .NET 9.0 AspNetCore runtime might not be installed!
    echo This could cause 502.5 errors in IIS.
    echo.
    echo You need to install the .NET 9.0 Runtime from:
    echo https://dotnet.microsoft.com/download/dotnet/9.0
) else (
    echo.
    echo SUCCESS: .NET 9.0 AspNetCore runtime is installed.
)

echo.
echo 3. Testing if IIS can access the dotnet executable...
where dotnet
if %errorlevel% neq 0 (
    echo.
    echo ERROR: dotnet executable not found in PATH!
    echo This will prevent IIS from launching your application.
    echo.
    echo Ensure the .NET SDK or Runtime is properly installed and in the system PATH.
) else (
    echo.
    echo SUCCESS: dotnet executable is in PATH.
)

echo.
echo 4. Checking Application Pool settings...
echo Running as IIS Admin account? Make sure application pools are set to:
echo    - No Managed Code (.NET CLR Version)
echo    - Appropriate identity with permissions to the application folders

echo.
echo 5. If you're still seeing 502.5 errors:
echo    a. Check the logs in the application's logs folder
echo    b. Run the test_app.bat script in each application folder
echo    c. Verify all required connection strings are correct
echo    d. Try setting ASPNETCORE_ENVIRONMENT to Development temporarily
echo    e. Check Event Viewer (Windows Logs -^> Application) for errors

pause
