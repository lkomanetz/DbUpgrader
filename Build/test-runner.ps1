ForEach ($folder in (Get-ChildItem -Path .\ -Directory -Filter *.Tests)) {
    Set-Location $folder.FullName
    dotnet test
}