@echo off
setlocal EnableDelayedExpansion

echo =========================================================
echo WebSpark Publishing and Optimization Script
echo =========================================================
echo Starting publishing process at %date% %time%
set start_time=%time%

echo Restoring NuGet packages for all projects...
dotnet restore C:\WebSpark\WebSpark.sln --verbosity quiet
if %errorlevel% neq 0 (
    echo ERROR: Package restore failed!
    pause
    exit /b 1
)

echo Building solution in Release mode first...
dotnet build C:\WebSpark\WebSpark.sln -c Release --no-restore --verbosity quiet
if %errorlevel% neq 0 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo Deleting all files and subdirectories in C:\PublishedWebsites...
rd /s /q C:\PublishedWebsites 2>nul
mkdir C:\PublishedWebsites
echo Deleted all files and subdirectories in C:\PublishedWebsites.

echo Publishing WebSpark.Web...
echo [1/3] Starting WebSpark.Web publish at %time%
dotnet publish C:\WebSpark\WebSpark.Web -c Release -o c:\PublishedWebsites\WebSpark.Web --no-self-contained --no-restore -p:PublishReadyToRun=false -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=false -p:PublishTrimmed=false --nologo > c:\PublishedWebsites\WebSparkWeb_PublishLog.txt 2>&1
if %errorlevel% neq 0 (
    echo ERROR: WebSpark.Web publish failed!
    type c:\PublishedWebsites\WebSparkWeb_PublishLog.txt
    pause
    exit /b 1
)
echo [1/3] WebSpark.Web published successfully at %time%

echo Publishing WebSpark.Portal...
echo [2/3] Starting WebSpark.Portal publish at %time%
dotnet publish C:\WebSpark\WebSpark.Portal -c Release -o c:\PublishedWebsites\WebSpark.Portal --no-self-contained --no-restore -p:PublishReadyToRun=false -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=false -p:PublishTrimmed=false --nologo > c:\PublishedWebsites\WebSparkPortal_PublishLog.txt 2>&1
if %errorlevel% neq 0 (
    echo ERROR: WebSpark.Portal publish failed!
    type c:\PublishedWebsites\WebSparkPortal_PublishLog.txt
    pause
    exit /b 1
)
echo [2/3] WebSpark.Portal published successfully at %time%

echo Publishing DataSpark.Web...
echo [3/3] Starting DataSpark.Web publish at %time%
dotnet publish C:\WebSpark\DataSpark.Web -c Release -o c:\PublishedWebsites\DataSpark.Web --no-self-contained --no-restore -p:PublishReadyToRun=false -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=false -p:PublishTrimmed=false -p:WarningsNotAsErrors=NU1605 --nologo > c:\PublishedWebsites\DataSparkWeb_PublishLog.txt 2>&1
if %errorlevel% neq 0 (
    echo ERROR: DataSpark.Web publish failed!
    type c:\PublishedWebsites\DataSparkWeb_PublishLog.txt
    pause
    exit /b 1
)
echo [3/3] DataSpark.Web published successfully at %time%

echo.
echo Cleaning up unnecessary files to reduce size...
echo Starting cleanup at %time%
set /a removed_files=0

echo Removing cross-platform native libraries (keeping only Windows versions)...
for /r "C:\PublishedWebsites" %%i in (*.dylib *.so) do (
    del /q "%%i" 2>nul && set /a removed_files+=1
)

echo Removing non-Windows runtime folders...
for %%p in (WebSpark.Web WebSpark.Portal DataSpark.Web) do (
    if exist "C:\PublishedWebsites\%%p\runtimes" (
        echo   Cleaning %%p runtimes folder...
        for /d %%d in ("C:\PublishedWebsites\%%p\runtimes\*") do (
            echo     Processing %%d
            :: Keep only win-x64 runtime folders
            echo "%%d" | findstr /i /c:"win-x64" > nul
            if errorlevel 1 (
                echo     Removing non-Windows runtime: %%d
                rd /s /q "%%d" 2>nul && set /a removed_files+=1
            )
        )
    )
)

echo Removing localization files (except en)...
for %%p in (WebSpark.Web WebSpark.Portal DataSpark.Web) do (
    for /d %%d in ("C:\PublishedWebsites\%%p\??", "C:\PublishedWebsites\%%p\??-??") do (
        echo "%%d" | findstr /i /c:"en" /c:"en-US" > nul
        if errorlevel 1 (
            echo Removing localization folder: %%d
            rd /s /q "%%d" 2>nul && set /a removed_files+=1
        )
    )
)

echo Removing debug and development files...
for %%p in (WebSpark.Web WebSpark.Portal DataSpark.Web) do (
    if exist "C:\PublishedWebsites\%%p\BlazorDebugProxy" (
        echo Removing BlazorDebugProxy from %%p
        rd /s /q "C:\PublishedWebsites\%%p\BlazorDebugProxy" 2>nul && set /a removed_files+=1
    )
)

echo Removing debug symbols...
for /r "C:\PublishedWebsites" %%i in (*.pdb) do (
    del /q "%%i" 2>nul && set /a removed_files+=1
)

echo Removing XML documentation files...
for /r "C:\PublishedWebsites" %%i in (*.xml) do (
    del /q "%%i" 2>nul && set /a removed_files+=1
)

echo Removing native lib files not needed for Windows IIS...
for /r "C:\PublishedWebsites" %%i in (*.a *.node) do (
    del /q "%%i" 2>nul && set /a removed_files+=1
)

echo Removing .deps.json and .runtimeconfig.dev.json files...
for /r "C:\PublishedWebsites" %%i in (*.runtimeconfig.dev.json) do (
    del /q "%%i" 2>nul && set /a removed_files+=1
)

echo Removing unnecessary dependency and reference assemblies...
for /r "C:\PublishedWebsites" %%i in (ref\*.dll) do (
    del /q "%%i" 2>nul && set /a removed_files+=1
)

echo Removing analyzers and build support files...
for %%p in (WebSpark.Web WebSpark.Portal DataSpark.Web) do (
    if exist "C:\PublishedWebsites\%%p\cs" rd /s /q "C:\PublishedWebsites\%%p\cs" 2>nul && set /a removed_files+=1
    if exist "C:\PublishedWebsites\%%p\de" rd /s /q "C:\PublishedWebsites\%%p\de" 2>nul && set /a removed_files+=1
    if exist "C:\PublishedWebsites\%%p\es" rd /s /q "C:\PublishedWebsites\%%p\es" 2>nul && set /a removed_files+=1
    if exist "C:\PublishedWebsites\%%p\fr" rd /s /q "C:\PublishedWebsites\%%p\fr" 2>nul && set /a removed_files+=1
    if exist "C:\PublishedWebsites\%%p\it" rd /s /q "C:\PublishedWebsites\%%p\it" 2>nul && set /a removed_files+=1
    if exist "C:\PublishedWebsites\%%p\ja" rd /s /q "C:\PublishedWebsites\%%p\ja" 2>nul && set /a removed_files+=1
    if exist "C:\PublishedWebsites\%%p\ko" rd /s /q "C:\PublishedWebsites\%%p\ko" 2>nul && set /a removed_files+=1
    if exist "C:\PublishedWebsites\%%p\pl" rd /s /q "C:\PublishedWebsites\%%p\pl" 2>nul && set /a removed_files+=1
    if exist "C:\PublishedWebsites\%%p\pt-BR" rd /s /q "C:\PublishedWebsites\%%p\pt-BR" 2>nul && set /a removed_files+=1
    if exist "C:\PublishedWebsites\%%p\ru" rd /s /q "C:\PublishedWebsites\%%p\ru" 2>nul && set /a removed_files+=1
    if exist "C:\PublishedWebsites\%%p\tr" rd /s /q "C:\PublishedWebsites\%%p\tr" 2>nul && set /a removed_files+=1
    if exist "C:\PublishedWebsites\%%p\zh-Hans" rd /s /q "C:\PublishedWebsites\%%p\zh-Hans" 2>nul && set /a removed_files+=1
    if exist "C:\PublishedWebsites\%%p\zh-Hant" rd /s /q "C:\PublishedWebsites\%%p\zh-Hant" 2>nul && set /a removed_files+=1
)

echo Removing unused ML.NET files from DataSpark.Web...
if exist "C:\PublishedWebsites\DataSpark.Web\runtimes" (
    for /d %%d in ("C:\PublishedWebsites\DataSpark.Web\runtimes\win-x86", "C:\PublishedWebsites\DataSpark.Web\runtimes\win-arm", "C:\PublishedWebsites\DataSpark.Web\runtimes\win-arm64") do (
        if exist "%%d" (
            rd /s /q "%%d" 2>nul && set /a removed_files+=1
        )
    )

    rem Clean up large ML.NET native libraries that aren't necessary for basic functionality
    echo Removing large ML.NET native libraries to save space...
    if exist "C:\PublishedWebsites\DataSpark.Web\runtimes\win-x64\native" (
        echo Cleaning ML.NET native libraries in DataSpark.Web...
        del /q "C:\PublishedWebsites\DataSpark.Web\runtimes\win-x64\native\MklImports.dll" 2>nul && set /a removed_files+=1
        del /q "C:\PublishedWebsites\DataSpark.Web\runtimes\win-x64\native\CpuMathNative.dll" 2>nul && set /a removed_files+=1
        del /q "C:\PublishedWebsites\DataSpark.Web\runtimes\win-x64\native\FastTreeNative.dll" 2>nul && set /a removed_files+=1
        del /q "C:\PublishedWebsites\DataSpark.Web\runtimes\win-x64\native\LdaNative.dll" 2>nul && set /a removed_files+=1
        del /q "C:\PublishedWebsites\DataSpark.Web\runtimes\win-x64\native\SymSgdNative.dll" 2>nul && set /a removed_files+=1
        del /q "C:\PublishedWebsites\DataSpark.Web\runtimes\win-x64\native\MatrixFactorizationNative.dll" 2>nul && set /a removed_files+=1
        del /q "C:\PublishedWebsites\DataSpark.Web\runtimes\win-x64\native\StockportClientWrapper.dll" 2>nul && set /a removed_files+=1
        del /q "C:\PublishedWebsites\DataSpark.Web\runtimes\win-x64\native\libonnxruntime.dll" 2>nul && set /a removed_files+=1
        del /q "C:\PublishedWebsites\DataSpark.Web\runtimes\win-x64\native\TensorFlow*.dll" 2>nul && set /a removed_files+=1
        del /q "C:\PublishedWebsites\DataSpark.Web\runtimes\win-x64\native\*.dll" 2>nul && set /a removed_files+=1
    )
    
    rem Remove ML.NET libraries from all projects if not critical for functionality
    for %%p in (WebSpark.Web WebSpark.Portal) do (
        if exist "C:\PublishedWebsites\%%p\runtimes\win-x64\native" (
            echo Cleaning ML.NET native libraries in %%p...
            del /q "C:\PublishedWebsites\%%p\runtimes\win-x64\native\MklImports.dll" 2>nul && set /a removed_files+=1
            del /q "C:\PublishedWebsites\%%p\runtimes\win-x64\native\CpuMathNative.dll" 2>nul && set /a removed_files+=1
            del /q "C:\PublishedWebsites\%%p\runtimes\win-x64\native\FastTreeNative.dll" 2>nul && set /a removed_files+=1
            del /q "C:\PublishedWebsites\%%p\runtimes\win-x64\native\LdaNative.dll" 2>nul && set /a removed_files+=1
            del /q "C:\PublishedWebsites\%%p\runtimes\win-x64\native\SymSgdNative.dll" 2>nul && set /a removed_files+=1
            del /q "C:\PublishedWebsites\%%p\runtimes\win-x64\native\MatrixFactorizationNative.dll" 2>nul && set /a removed_files+=1
            del /q "C:\PublishedWebsites\%%p\runtimes\win-x64\native\StockportClientWrapper.dll" 2>nul && set /a removed_files+=1
            del /q "C:\PublishedWebsites\%%p\runtimes\win-x64\native\libonnxruntime.dll" 2>nul && set /a removed_files+=1
            del /q "C:\PublishedWebsites\%%p\runtimes\win-x64\native\TensorFlow*.dll" 2>nul && set /a removed_files+=1
        )
    )
)

echo Compressing static assets...
for %%p in (WebSpark.Web WebSpark.Portal DataSpark.Web) do (
    if exist "C:\PublishedWebsites\%%p\wwwroot" (
        echo Optimizing %%p wwwroot folder...
        
        rem Remove unnecessary static files
        if exist "C:\PublishedWebsites\%%p\wwwroot\lib" (
            for /d %%d in ("C:\PublishedWebsites\%%p\wwwroot\lib\*") do (
                if exist "%%d\dist" (
                    for /d %%s in ("%%d\src", "%%d\node_modules", "%%d\examples", "%%d\docs", "%%d\test", "%%d\demo", "%%d\.github", "%%d\build") do (
                        if exist "%%s" rd /s /q "%%s" 2>nul && set /a removed_files+=1
                    )
                )
            )
        )
        
        rem Remove source maps
        for /r "C:\PublishedWebsites\%%p\wwwroot" %%i in (*.map) do (
            del /q "%%i" 2>nul && set /a removed_files+=1
        )
        
        rem Remove development versions of JS/CSS files
        for /r "C:\PublishedWebsites\%%p\wwwroot" %%i in (*-dev*, *.min.js.LICENSE.txt, *umd.js, *.esm.js) do (
            del /q "%%i" 2>nul && set /a removed_files+=1
        )
        
        rem Remove unnecessary files from popular libraries
        if exist "C:\PublishedWebsites\%%p\wwwroot\lib\bootstrap" (
            for /d %%s in ("C:\PublishedWebsites\%%p\wwwroot\lib\bootstrap\dist\js") do (
                if exist "%%s" (
                    for %%f in ("%%s\*.js") do (
                        echo "%%f" | findstr /i /c:"bootstrap.min.js" /c:"bootstrap.bundle.min.js" > nul
                        if errorlevel 1 (
                            del /q "%%f" 2>nul && set /a removed_files+=1
                        )
                    )
                )
            )
        )
        
        if exist "C:\PublishedWebsites\%%p\wwwroot\lib\jquery" (
            for /d %%s in ("C:\PublishedWebsites\%%p\wwwroot\lib\jquery\dist") do (
                if exist "%%s" (
                    for %%f in ("%%s\*.js") do (
                        echo "%%f" | findstr /i /c:"jquery.min.js" > nul
                        if errorlevel 1 (
                            del /q "%%f" 2>nul && set /a removed_files+=1
                        )
                    )
                )
            )
        )
        
        rem Remove unused image files (adjust patterns as needed)
        for /r "C:\PublishedWebsites\%%p\wwwroot\images" %%i in (*.psd, *.ai, *.svg, *-large*.*, *-original*.*) do (
            del /q "%%i" 2>nul && set /a removed_files+=1
        )
        
        rem Remove any sample/demo/test data files
        for /r "C:\PublishedWebsites\%%p\wwwroot" %%i in (sample*.json, demo*.json, test*.json, example*.json) do (
            del /q "%%i" 2>nul && set /a removed_files+=1
        )
    )
)

echo Optimizing web.config files for IIS...
for %%p in (WebSpark.Web WebSpark.Portal DataSpark.Web) do (
    if exist "C:\PublishedWebsites\%%p\web.config" (
        powershell -Command "(Get-Content 'C:\PublishedWebsites\%%p\web.config') -replace 'stdoutLogEnabled=\"false\"', 'stdoutLogEnabled=\"true\"' -replace 'hostingModel=\"inprocess\"', 'hostingModel=\"OutOfProcess\"' | Set-Content 'C:\PublishedWebsites\%%p\web.config'"
    )
)

echo Identifying and removing large unused files...
echo This step helps reduce package size significantly...

rem Remove any unused or duplicate assemblies that might have been included
for %%p in (WebSpark.Web WebSpark.Portal DataSpark.Web) do (
    if exist "C:\PublishedWebsites\%%p" (
        echo Finding and removing large unused files in %%p...
        
        rem List the largest files for reference
        powershell -Command "Get-ChildItem -Path 'C:\PublishedWebsites\%%p' -Recurse -File | Sort-Object Length -Descending | Select-Object -First 10 | Format-Table Name, @{Name='Size (MB)';Expression={'{0:N2}' -f ($_.Length / 1MB)}} -AutoSize"
        
        rem Remove specific large DLLs if they aren't critical (adjust as needed)
        for %%f in (
            System.Data.SqlClient.dll
            Azure.Core.dll
            Microsoft.Identity.Client.dll
            System.Configuration.ConfigurationManager.dll
            Newtonsoft.Json.dll
        ) do (
            if exist "C:\PublishedWebsites\%%p\%%f" (
                echo Checking if %%f is referenced in deps.json...
                powershell -Command "if ((Get-Content 'C:\PublishedWebsites\%%p\%%p.deps.json' -Raw) -notmatch '%%f') { Remove-Item 'C:\PublishedWebsites\%%p\%%f' -Force; Write-Host '  Removed unused %%f' }"
            )
        )
        
        rem Find duplicate DLLs with the same name in multiple folders
        powershell -Command "$files = Get-ChildItem -Path 'C:\PublishedWebsites\%%p' -Recurse -Include *.dll | Select-Object -ExpandProperty Name | Group-Object | Where-Object { $_.Count -gt 1 }; foreach ($file in $files) { Write-Host ('  Found duplicate: ' + $file.Name) }"
    )
)

echo Cleanup completed at %time% - Removed %removed_files% unnecessary files

echo Creating WebSpark.Zip with all folders under C:\PublishedWebsites...
echo Creating IIS-ready deployment packages at %time%...

echo Removing empty directories to clean up further...
powershell -Command "$emptyDirs = 0; $continue = $true; while ($continue) { $dirs = Get-ChildItem -Path 'C:\PublishedWebsites' -Directory -Recurse | Where-Object { !(Get-ChildItem -Path $_.FullName -Recurse -File) }; if ($dirs -and $dirs.Count -gt 0) { foreach ($dir in $dirs) { Remove-Item $dir.FullName -Force; $emptyDirs++ } } else { $continue = $false } }; Write-Host \"Removed $emptyDirs empty directories\""

echo Optimizing for IIS Deployment...
for %%p in (WebSpark.Web WebSpark.Portal DataSpark.Web) do (
    echo Creating IIS deployment package for %%p...
    
    rem Create a proper web.config file for IIS hosting
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
        echo           ^<environmentVariable name="DOTNET_PRINT_TELEMETRY_MESSAGE" value="false" /^>
        echo         ^</environmentVariables^>
        echo         ^<handlerSettings^>
        echo           ^<handlerSetting name="debugFile" value=".\logs\ancm.log" /^>
        echo           ^<handlerSetting name="debugLevel" value="FILE,TRACE" /^>
        echo         ^</handlerSettings^>
        echo       ^</aspNetCore^>
        echo     ^</system.webServer^>
        echo   ^</location^>
        echo ^</configuration^>
    ) > "C:\PublishedWebsites\%%p\web.config"
    
    if not exist "C:\PublishedWebsites\%%p\logs" mkdir "C:\PublishedWebsites\%%p\logs"
    
    rem Create a test script to ensure the application can run standalone
    echo @echo off > "C:\PublishedWebsites\%%p\test_app.bat"
    echo echo Testing %%p standalone execution... >> "C:\PublishedWebsites\%%p\test_app.bat"
    echo dotnet %%p.dll >> "C:\PublishedWebsites\%%p\test_app.bat"
    echo pause >> "C:\PublishedWebsites\%%p\test_app.bat"
    echo echo If the application started successfully, it can run outside IIS. >> "C:\PublishedWebsites\%%p\test_app.bat"
    echo echo Press Ctrl+C to exit. >> "C:\PublishedWebsites\%%p\test_app.bat"
)

powershell -Command "$params = @{ Path = 'C:\PublishedWebsites\*'; DestinationPath = 'C:\PublishedWebsites\WebSpark.Zip'; Force = $true; CompressionLevel = 'Optimal' }; Compress-Archive @params"
echo WebSpark.Zip created with maximum compression.

echo.
echo Publishing Summary:
echo ==================
echo Directory Sizes:
powershell "Get-ChildItem -Path 'C:\PublishedWebsites' -Directory | ForEach-Object { $size = (Get-ChildItem $_.FullName -Recurse | Measure-Object -Property Length -Sum).Sum; Write-Host ('{0}: {1:N2} MB' -f $_.Name, ($size / 1MB)) }"

echo.
echo Largest Files By Project:
for %%p in (WebSpark.Web WebSpark.Portal DataSpark.Web) do (
    echo Top 5 largest files in %%p:
    powershell -Command "Get-ChildItem -Path 'C:\PublishedWebsites\%%p' -Recurse -File | Sort-Object Length -Descending | Select-Object -First 5 | Format-Table @{Name='Path';Expression={$_.FullName.Replace('C:\PublishedWebsites\%%p\', '')}}, @{Name='Size (MB)';Expression={'{0:N2}' -f ($_.Length / 1MB)}} -AutoSize"
)

echo.
echo Zip File Size:
powershell "if (Test-Path 'C:\PublishedWebsites\WebSpark.Zip') { $zipSize = (Get-Item 'C:\PublishedWebsites\WebSpark.Zip').Length; Write-Host ('WebSpark.Zip: {0:N2} MB' -f ($zipSize / 1MB)) }"
echo.
echo IIS Deployment Information:
echo ==========================
echo To deploy to IIS:
echo 1. Create application pools for each app (WebSpark.Web, WebSpark.Portal, DataSpark.Web)
echo 2. Set each application pool to use .NET CLR Version: "No Managed Code"
echo 3. Create websites or applications pointing to C:\PublishedWebsites\[AppName]
echo 4. Ensure IIS has ASP.NET Core Module v2 installed
echo 5. If running on Windows Server, ensure Web Server (IIS) role is installed with all ASP.NET features
echo.

echo =========================================================
echo All web applications published and optimized.
echo Process completed at %date% %time%

set end_time=%time%

rem Calculate elapsed time
for /f "tokens=1-4 delims=:.," %%a in ("%start_time%") do (
   set /a "start=(((%%a*60)+1%%b %% 100)*60+1%%c %% 100)*100+1%%d %% 100"
)
for /f "tokens=1-4 delims=:.," %%a in ("%end_time%") do (
   set /a "end=(((%%a*60)+1%%b %% 100)*60+1%%c %% 100)*100+1%%d %% 100"
)
set /a elapsed=end-start
set /a hh=elapsed/(60*60*100), remainder=elapsed%%(60*60*100), mm=remainder/(60*100), remainder%%=60*100, ss=remainder/100, cc=remainder%%100
if %hh% lss 10 set hh=0%hh%
if %mm% lss 10 set mm=0%mm%
if %ss% lss 10 set ss=0%ss%
if %cc% lss 10 set cc=0%cc%

echo Total elapsed time: %hh%:%mm%:%ss%.%cc%
echo =========================================================

echo.
echo IIS Troubleshooting Guide for 502.5 - ANCM Out-Of-Process Startup Failure:
echo ===========================================================================
echo 1. Check log files in .\logs\ folder for each application to find the root cause
echo 2. Verify .NET Core Runtime is installed on the server (v9.0)
echo 3. Run test_app.bat in each application folder to test if it runs standalone
echo 4. Verify ASP.NET Core Module v2 is installed on IIS server
echo 5. Check Application Pool settings:
echo    - No Managed Code for .NET CLR Version
echo    - Identity has appropriate permissions
echo 6. Common causes of 502.5 errors:
echo    - Missing dependencies (especially if trimmed)
echo    - Incorrect connection strings
echo    - Permissions issues
echo    - Port conflicts
echo    - Missing environment variables
echo 7. For detailed logs, check:
echo    - Event Viewer (Windows Logs -^> Application)
echo    - stdout logs in the logs folder
echo    - ANCM logs in the logs folder
echo 8. Try setting ASPNETCORE_ENVIRONMENT to Development temporarily for more detailed errors
echo 9. Ensure AspNetCoreModuleV2 is installed using the command:
echo    dism /online /get-packages | findstr "Microsoft-Windows-IIS-AspNetCore"
echo ===========================================================================
pause
