# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/My_Reloaded_II_Mod1/*" -Force -Recurse
dotnet publish "./My_Reloaded_II_Mod1.csproj" -c Release -o "$env:RELOADEDIIMODS/My_Reloaded_II_Mod1" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location