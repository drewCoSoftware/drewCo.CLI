function Item-Exists($path) {

    $x = Get-Item -Path $path -ErrorAction SilentlyContinue
    return $x.Length -gt 0
}


# Get a clean copy of the build dir.
if (Item-Exists("nuget-build")) {
    Remove-Item "nuget-build" -Force -Recurse

    New-Item "nuget-build" -ItemType "Directory"
}
New-Item "nuget-build\lib\net8.0" -ItemType "Directory"

# Build our assemblies:
dotnet build drewCo.CLI.sln -p:Configuration=Release

# NETCORE 6.0
Copy-Item -Path ".\drewCo.CLI\bin\Release\net8.0\drewCo.CLI.dll" "nuget-build\lib\net8.0\drewCo.CLI.dll"
Copy-Item -Path ".\drewCo.CLI\bin\Release\net8.0\drewCo.CLI.pdb" "nuget-build\lib\net8.0\drewCo.CLI.pdb"
Copy-Item -Path ".\drewCo.CLI\bin\Release\net8.0\drewCo.CLI.xml" "nuget-build\lib\net8.0\drewCo.CLI.xml"
Copy-Item -Path ".\drewCo.CLI\bin\Release\net8.0\drewCo.CLI.deps.json" "nuget-build\lib\net8.0\drewCo.CLI.deps.json"


# Copy everything to the build dir....
# Copy-Item -Path ".\lib" ".\nuget-build\lib" -Recurse
Copy-Item ".\drewCo.CLI.nuspec" ".\nuget-build\drewCo.CLI.nuspec"

# Pack it all up...
nuget pack ".\nuget-build\drewCo.CLI.nuspec"