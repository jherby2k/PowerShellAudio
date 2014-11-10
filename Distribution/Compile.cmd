@ECHO OFF

PUSHD ..

"%ProgramFiles(x86)%\WiX Toolset v3.9\bin\candle.exe" "%~dp0PowerShellAudio.wsx"
IF %ERRORLEVEL% NEQ 0 GOTO Finish

"%ProgramFiles(x86)%\WiX Toolset v3.9\bin\light.exe" -ext WixUIExtension -cultures:en-us PowerShellAudio.wixobj
IF %ERRORLEVEL% NEQ 0 GOTO Finish

"%ProgramFiles(x86)%\WiX Toolset v3.9\bin\candle.exe" -arch x64 "%~dp0PowerShellAudio64.wsx"
IF %ERRORLEVEL% NEQ 0 GOTO Finish

"%ProgramFiles(x86)%\WiX Toolset v3.9\bin\light.exe" -ext WixUIExtension -cultures:en-us PowerShellAudio64.wixobj
IF %ERRORLEVEL% NEQ 0 GOTO Finish

"%ProgramFiles(x86)%\WiX Toolset v3.9\bin\candle.exe" -ext WixBalExtension -ext WixUtilExtension -ext WixNetFxExtension "%~dp0PowerShellAudioInstaller.wsx"
IF %ERRORLEVEL% NEQ 0 GOTO Finish

"%ProgramFiles(x86)%\WiX Toolset v3.9\bin\light.exe" -ext WixBalExtension -ext WixUtilExtension -ext WixNetFxExtension PowerShellAudioInstaller.wixobj

:Finish
DEL PowerShellAudio.wixobj
DEL PowerShellAudio.wixpdb
DEL PowerShellAudio64.wixobj
DEL PowerShellAudio64.wixpdb
DEL PowerShellAudioInstaller.wixobj
DEL PowerShellAudioInstaller.wixpdb
POPD