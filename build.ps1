$baseDir = (Get-Item -Path ".\" -Verbose).FullName
$items = Get-ChildItem -Path $baseDir -Include *.sln -Recurse
foreach ($item in $items){
    Write-Output "Building $($item.FullName)"
    dotnet build $item
    Write-Output ""
}
