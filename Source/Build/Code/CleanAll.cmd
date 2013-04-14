CALL "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86

MSBUILD /t:Clean /p:Configuration=Debug ..\..\Code\CannedBytes.sln
MSBUILD /t:Clean /p:Configuration=Release ..\..\Code\CannedBytes.sln
