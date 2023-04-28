dotnet publish LogicCircuit\LogicCircuit.csproj --sc -c Release -r win-x64 -o LogicCircuit\bin\Release_64\Publish
dotnet publish LogicCircuit\LogicCircuit.csproj --sc -c Release -r win-x86 -o LogicCircuit\bin\Release_32\Publish

msbuild -r -p:Configuration=Release;Platform=x64 Setup\Setup.wixproj
msbuild -r -p:Configuration=Release;Platform=x86 Setup\Setup.wixproj
