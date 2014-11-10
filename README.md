PowerShell Audio
==========

An extensible, multi-format audio conversion and tagging module for Windows PowerShell

## Getting Help
Because PowerShell Audio is a Windows PowerShell module, help is integrated. Simply launch PowerShell, then use the Get-Help cmdlet. For example:
> Get-Help Export-AudioFile -Full

If you are new to Windows PowerShell, here is a quick primer to get you up and running:
https://github.com/jherby2k/PowerShellAudio/wiki/PowerShell-Primer

## Examples

This command simply converts a folder full of FLAC files into MP3s:
> Get-AudioFile *.flac  | Export-AudioFile "Lame MP3" -Directory C:\Output

The Lame encoder uses VBR quality level 3 by default. To list the defaults:
> Get-AudioEncoderDefaultSettingList "Lame MP3"

To get all the available settings for Lame:
> Get-AudioEncoderAvailableSettingList "Lame MP3"

Add ReplayGain to your entire FLAC library, treating each directory as a separate album:
> Get-ChildItem C:\Users\Myself\Music -Directory -Recurse | % { $_ | Get-ChildItem -File -Filter *.flac | Measure-AudioFile ReplayGain -PassThru | Save-AudioMetadata }

Convert your whole FLAC library to VBR AAC, with SoundCheck tags calculated from album ReplayGain information:
> Get-ChildItem C:\Users\Myself\Music -Filter *.flac -Recurse | Get-AudioFile | Export-AudioFile "Apple AAC" -Directory "C:\Output\{Artist}\{Album}" -Setting @{AddSoundCheck = "Album"} -Name "{TrackNumber} - {Title}"

Convert your whole FLAC library to VBR MP3, with ReplayGain directly applied to the resulting volume levels:
> Get-ChildItem C:\Users\Myself\Music -Filter *.flac -Recurse | Get-AudioFile | Export-AudioFile "Lame MP3" -Directory "C:\Output\{Artist}\{Album}" -Setting @{ApplyGain = "Album"} -Name "{TrackNumber} - {Title}"

## Prerequisites for Building
1. Windows 7 or Windows 8.1.
2. Visual Studio Express 2013 for Windows Desktop (free), or any higher version of Visual Studio 2013.
3. Windows Management Framework 4.0 (if you are building on Windows 7).
4. Code Contracts for .NET (download from the Visual Studio Gallery). Note that the properties page is not available in Visual Studio Express, but the functionality is still there.
5. The WiX Toolset v3.9 or higher, if you want to build the deployment files.
