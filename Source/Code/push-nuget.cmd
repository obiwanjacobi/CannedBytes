msbuild CannedBytes.sln /t:Build /p:Configuration=Release

nuget push CannedBytes\bin\Release\CannedBytes.2.0.1.nupkg -src https://api.nuget.org/v3/index.json
nuget push CannedBytes.Media\bin\Release\CannedBytes.Media.2.0.1.nupkg -src https://api.nuget.org/v3/index.json
nuget push CannedBytes.Media.IO\bin\Release\CannedBytes.Media.IO.2.0.1.nupkg -src https://api.nuget.org/v3/index.json
