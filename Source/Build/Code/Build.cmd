CALL "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86

MSBUILD /m /t:Clean /p:Configuration=%1 ..\..\Code\CannedBytes.sln
MSBUILD /m /p:Configuration=%1 ..\..\Code\CannedBytes.sln
