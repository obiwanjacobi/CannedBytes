CALL "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86

MSBUILD /p:Configuration=Debug ..\..\Code\CannedBytes.sln
MSBUILD /p:Configuration=Release ..\..\Code\CannedBytes.sln
