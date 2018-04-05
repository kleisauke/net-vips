:: Note: this script should be executed from the root directory

git clean -xfd
dotnet restore
dotnet build .\src\NetVips\NetVips.csproj -c Release
dotnet pack .\src\NetVips\NetVips.csproj -c Release
rmdir artifacts /s /q
mkdir artifacts
for /R %%x in (BenchmarkDotNet*.nupkg) do copy "%%x" "artifacts/" /Y