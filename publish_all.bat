@echo off

echo Deleting all files and subdirectories in C:\PublishedWebsites...
rd /s /q C:\PublishedWebsites
mkdir C:\PublishedWebsites
echo Deleted all files and subdirectories in C:\PublishedWebsites.

echo Publishing WebSpark.Web...
dotnet publish C:\GitHub\MarkHazleton\WebSpark\WebSpark.Web -p:PublishProfile=FolderProfile -o c:\PublishedWebsites\WebSpark > c:\PublishedWebsites\WebSparkWeb_PublishLog.txt 2>&1
echo WebSpark.Web published.

echo Publishing WebSpark.Portal...
dotnet publish C:\GitHub\MarkHazleton\WebSpark\WebSpark.Portal -p:PublishProfile=FolderProfile -o c:\PublishedWebsites\WebSpark.Portal > c:\PublishedWebsites\WebSparkPortal_PublishLog.txt 2>&1
echo WebSpark.Portal published.

echo Publishing PromptSpark.Chat...
dotnet publish C:\GitHub\MarkHazleton\WebSpark\PromptSpark.Chat -p:PublishProfile=FolderProfile -o c:\PublishedWebsites\PromptSpark.Chat > c:\PublishedWebsites\PromptSparkChat_PublishLog.txt 2>&1
echo PromptSpark.Chat published.

echo Creating WebSpark.Zip with all folders under C:\PublishedWebsites...
powershell Compress-Archive -Path C:\PublishedWebsites\* -DestinationPath C:\PublishedWebsites\WebSpark.Zip
echo WebSpark.Zip created.

echo All web applications published and zipped.
pause
