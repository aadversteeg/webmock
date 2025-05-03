param (
    [string]$Configuration = "Release",
    [string]$VersionSuffix = "",
    [string]$OutputPath = "./artifacts"
)

# Create output directory if it doesn't exist
if (!(Test-Path -Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

# Clean
Write-Host "Cleaning solution..." -ForegroundColor Green
dotnet clean ./src/webmock.sln -c $Configuration
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Green
dotnet restore ./src/webmock.sln
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Build
Write-Host "Building solution..." -ForegroundColor Green
dotnet build ./src/webmock.sln -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Run tests
Write-Host "Running tests..." -ForegroundColor Green
dotnet test ./src/webmock.sln -c $Configuration --no-build
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Create NuGet package
Write-Host "Creating NuGet package..." -ForegroundColor Green
$PackageArgs = @("pack", "./src/Ave.WebMock/Ave.WebMock.csproj", "-c", $Configuration, "--no-build", "-o", $OutputPath)

if ($VersionSuffix) {
    $PackageArgs += "--version-suffix"
    $PackageArgs += $VersionSuffix
}

dotnet $PackageArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "NuGet package created successfully in $OutputPath" -ForegroundColor Green