PowerShell Audio
==========

An extensible, multi-format audio conversion and tagging module for Windows PowerShell

## Getting Help
[Visit the wiki](https://github.com/jherby2k/PowerShellAudio/wiki).

## Prerequisites for Building
1. Windows 7 or Windows 8.1.
2. Visual Studio Community Edition (free), or any higher version of Visual Studio 2013.
3. Windows Management Framework 4.0 (if you are building on Windows 7).
4. The Code Contracts extension (http://research.microsoft.com/en-us/projects/contracts)
5. The WiX Toolset extension v3.9 (http://wixtoolset.org)

## Acknowledgements
This project uses libraries from several high-quality open source projects.
* [Lame](http://lame.sourceforge.net/) - The highest quality MP3 encoder available.
* [FLAC](https://xiph.org/flac/) - The most popular lossless codec around.
* [Ogg Vorbis](http://www.vorbis.com/) - The first patent-free open source codec.
* [aoTuv](http://www.geocities.jp/aoyoume/aotuv/) - Quality enhancements to the standard libvorbis.
* [C# ID3 Library](https://sourceforge.net/projects/csid3lib/)
* [libebur128](https://github.com/jiixyj/libebur128) - A free R128 (ReplayGain 2.0) implementation.

Code and documentation from the following sources was also extremely instructive and helpful:
* [qaac](https://github.com/nu774/qaac) - The "other" front-end for accessing the Apple encoders.
* [ReplayGain 1.0](http://wiki.hydrogenaud.io/index.php?title=ReplayGain_specification)
* [ReplayGain 2.0](http://wiki.hydrogenaud.io/index.php?title=ReplayGain_2.0_specification)
* [AtomicParsley](http://atomicparsley.sourceforge.net/) - Details about the MP4 file format and iTunes metadata.
* [ID3.org](http://id3.org/) - Really detailed documentation for the ID3 specification.
* [The Sonic Spot](http://www.sonicspot.com/guide/wavefiles.html) - Great information about the WAVE file format.
