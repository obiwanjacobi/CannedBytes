REM Copy assembly files from their projects directories into the package folder structure

xcopy /E /Y ..\..\Code\CannedBytes\bin\%1\CannedBytes.* ..\_Packages\Code\%1\
xcopy /E /Y ..\..\Code\CannedBytes.IO\bin\%1\CannedBytes.IO.* ..\_Packages\Code\%1\
xcopy /E /Y ..\..\Code\CannedBytes.Media\bin\%1\CannedBytes.Media.* ..\_Packages\Code\%1\
xcopy /E /Y ..\..\Code\CannedBytes.Media.IO\bin\%1\CannedBytes.Media.IO.* ..\_Packages\Code\%1\

REM Cleanup some garbage that came with it

Del ..\_Packages\Code\%1\*.old
Del ..\_Packages\Code\%1\*.CodeAnalysisLog.xml
Del ..\_Packages\Code\%1\*.lastcodeanalysissucceeded
