@echo off
echo =========================================================
echo ASP.NET Core Application Config Check
echo =========================================================
echo This script checks common configuration issues that might cause 502.5 errors.

set APP_PATH=%1
if "%APP_PATH%"=="" (
    echo Please provide the path to the published application.
    echo Example: check_app_config.bat C:\PublishedWebsites\WebSpark.Web
    exit /b 1
)

if not exist "%APP_PATH%" (
    echo Path %APP_PATH% does not exist!
    exit /b 1
)

echo.
echo Checking application at: %APP_PATH%
echo.

echo 1. Checking for required files...
set MISSING_FILES=0

if not exist "%APP_PATH%\web.config" (
    echo ERROR: web.config not found!
    set /a MISSING_FILES+=1
) else (
    echo - web.config found
    type "%APP_PATH%\web.config" | findstr "aspNetCore" > nul
    if %errorlevel% neq 0 (
        echo   WARNING: web.config might not be configured correctly for ASP.NET Core
    )
)

for %%f in ("%APP_PATH%\*.dll") do (
    set FOUND_DLL=1
)
if not defined FOUND_DLL (
    echo ERROR: No DLL files found in the application folder!
    set /a MISSING_FILES+=1
) else (
    echo - DLL files found
)

echo.
echo 2. Checking for appsettings.json...
if not exist "%APP_PATH%\appsettings.json" (
    echo WARNING: appsettings.json not found. This might be intentional but could cause issues.
) else (
    echo - appsettings.json found
)

echo.
echo 3. Checking logs directory...
if not exist "%APP_PATH%\logs" (
    echo WARNING: logs directory not found. Creating it...
    mkdir "%APP_PATH%\logs"
) else (
    echo - logs directory exists
    if exist "%APP_PATH%\logs\stdout*" (
        echo - Found stdout logs, check them for errors:
        dir "%APP_PATH%\logs\stdout*"
    )
    if exist "%APP_PATH%\logs\ancm*" (
        echo - Found ANCM logs, check them for errors:
        dir "%APP_PATH%\logs\ancm*"
    )
)

echo.
echo 4. Testing if application can run standalone...
echo Running dotnet command to test application...
pushd "%APP_PATH%"
echo dotnet "%APP_PATH%\*.dll" --urls http://localhost:5000 --environment Production
echo Testing application for 5 seconds...
start /b cmd /c "dotnet ""%APP_PATH%\*.dll"" --urls http://localhost:5000 --environment Production > ""%APP_PATH%\logs\standalone_test.log"" 2>&1"
timeout /t 5 /nobreak > nul
taskkill /f /im dotnet.exe > nul 2>&1
popd
echo Test completed. Check %APP_PATH%\logs\standalone_test.log for results.

echo.
echo 5. Recommendations:
echo - Ensure ASP.NET Core Module v2 is installed in IIS (run check_aspnetcore_iis.bat)
echo - Verify application pool is set to "No Managed Code" and has correct permissions
echo - Check connection strings and other configuration in appsettings.json
echo - Check logs in the logs directory for specific error details
echo - Try modifying web.config to set ASPNETCORE_ENVIRONMENT to Development temporarily

pause
