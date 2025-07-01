@echo off
echo =========================================================
echo WebSpark Minimal IIS Deployment Helper
echo =========================================================
echo This script creates a minimal deployment without trimming or optimization
echo to resolve 502.5 ANCM Out-Of-Process Startup Failure issues.

echo.
echo Creating minimal deployment directory...
if not exist C:\MinimalPublish mkdir C:\MinimalPublish

echo.
echo Publishing applications with minimal optimization...
for %%p in (WebSpark.Web WebSpark.Portal DataSpark.Web) do (
    echo.
    echo Publishing %%p...
    dotnet publish C:\WebSpark\%%p -c Release -o c:\MinimalPublish\%%p --no-self-contained
    
    echo Creating enhanced web.config for %%p...
    (
        echo ^<?xml version="1.0" encoding="utf-8"?^>
        echo ^<configuration^>
        echo   ^<location path="."^>
        echo     ^<system.webServer^>
        echo       ^<handlers^>
        echo         ^<add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" /^>
        echo       ^</handlers^>
        echo       ^<aspNetCore processPath="dotnet" arguments=".\%%p.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="OutOfProcess"^>
        echo         ^<environmentVariables^>
        echo           ^<environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" /^>
        echo         ^</environmentVariables^>
        echo         ^<handlerSettings^>
        echo           ^<handlerSetting name="debugFile" value=".\logs\ancm.log" /^>
        echo           ^<handlerSetting name="debugLevel" value="FILE,TRACE" /^>
        echo         ^</handlerSettings^>
        echo       ^</aspNetCore^>
        echo     ^</system.webServer^>
        echo   ^</location^>
        echo ^</configuration^>
    ) > "C:\MinimalPublish\%%p\web.config"
    
    if not exist "C:\MinimalPublish\%%p\logs" mkdir "C:\MinimalPublish\%%p\logs"
    
    echo Creating test batch file for %%p...
    (
        echo @echo off
        echo echo Testing %%p standalone execution...
        echo dotnet %%p.dll
        echo pause
    ) > "C:\MinimalPublish\%%p\test_app.bat"
)

echo.
echo =========================================================
echo Minimal deployment completed! 
echo Applications are in C:\MinimalPublish\
echo.
echo To deploy to IIS:
echo 1. Create application pools with "No Managed Code" setting
echo 2. Create websites/applications pointing to C:\MinimalPublish\[AppName]
echo 3. Ensure ASP.NET Core Module v2 is installed
echo 4. Run test_app.bat in each folder to verify standalone execution
echo =========================================================

pause
