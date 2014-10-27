@ECHO OFF

PUSHD ..

"%ProgramFiles(x86)%\WiX Toolset v3.9\bin\candle.exe" "%~dp0AudioShell.wsx"
IF %ERRORLEVEL% NEQ 0 GOTO Finish

"%ProgramFiles(x86)%\WiX Toolset v3.9\bin\light.exe" -ext WixUIExtension -cultures:en-us AudioShell.wixobj
IF %ERRORLEVEL% NEQ 0 GOTO Finish

"%ProgramFiles(x86)%\WiX Toolset v3.9\bin\candle.exe" -arch x64 "%~dp0AudioShell64.wsx"
IF %ERRORLEVEL% NEQ 0 GOTO Finish

"%ProgramFiles(x86)%\WiX Toolset v3.9\bin\light.exe" -ext WixUIExtension -cultures:en-us AudioShell64.wixobj
IF %ERRORLEVEL% NEQ 0 GOTO Finish

"%ProgramFiles(x86)%\WiX Toolset v3.9\bin\candle.exe" -ext WixBalExtension -ext WixUtilExtension -ext WixNetFxExtension "%~dp0AudioShellBundle.wsx"
IF %ERRORLEVEL% NEQ 0 GOTO Finish

"%ProgramFiles(x86)%\WiX Toolset v3.9\bin\light.exe" -ext WixBalExtension -ext WixUtilExtension -ext WixNetFxExtension AudioShellBundle.wixobj

:Finish
DEL AudioShell.wixobj
DEL AudioShell.wixpdb
DEL AudioShell64.wixobj
DEL AudioShell64.wixpdb
DEL AudioShellBundle.wixobj
DEL AudioShellBundle.wixpdb
POPD