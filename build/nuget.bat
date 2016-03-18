cls
rd ..\dist\nuget /s /q
md ..\dist
md ..\dist\nuget

cd ..\src\ProviderModel
..\..\.nuget\NuGet.exe pack ..\ProviderModel\ProviderModel.csproj -Build -MSBuildVersion 4 -Prop Configuration=Release -OutputDirectory ..\..\dist\nuget
cd ..\..\build

pause