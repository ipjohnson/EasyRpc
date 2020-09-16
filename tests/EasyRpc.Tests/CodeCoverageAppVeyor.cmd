@echo off

SET dotnet="C:/Program Files/dotnet/dotnet.exe"  
SET opencover=..\..\tools\OpenCover.4.7.922\tools\OpenCover.Console.exe  
SET coveralls=..\..\tools\coveralls.net.2.0.0-beta0002\tools\csmacnz.Coveralls.exe

SET targetargs="test"  
SET filter="+[*]EasyRpc.* -[EasyRpc.Tests]* -[xunit.*]* "  
SET coveragefile=Coverage.xml  
SET coveragedir=Coverage

dotnet install OpenCover -Version 4.7.922 -OutputDirectory ..\..\tools
dotnet install coveralls.net -Version 2.0.0-beta0002 -OutputDirectory ..\..\tools

REM Run code coverage analysis  
%opencover% -oldStyle -returntargetcode -register:user -target:%dotnet% -output:%coveragefile% -targetargs:%targetargs% -filter:%filter% -skipautoprops -hideskipped:All

IF %ERRORLEVEL% NEQ 0 (
    Echo Unit tests failed
	EXIT /B %ERRORLEVEL%
)

REM publish
IF NOT "%COVERALLS_REPO_TOKEN%"=="" %coveralls% --serviceName appveyor --opencover -i .\Coverage.xml
