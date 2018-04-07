:: Note: this script should be executed from the root directory

git clean -xfd
dotnet restore
dotnet build .\build\NetVips.batch.csproj
dotnet pack .\src\NetVips\NetVips.csproj -c Release /p:TargetOS=Windows /p:Platform="Any CPU"
rmdir artifacts /s /q
mkdir artifacts
for /R %%x in (NetVips*.nupkg) do copy "%%x" "artifacts/" /Y