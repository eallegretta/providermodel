cls
rd ..\dist\nuget /s /q
md ..\dist
md ..\dist\nuget

cd ..\src\ProviderModel
..\..\tools\NuGet.exe pack -Build -Prop Configuration=Release -OutputDirectory ..\..\dist\nuget
cd ..\..\build

pause