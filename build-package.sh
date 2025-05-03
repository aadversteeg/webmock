#!/bin/bash

# Default parameters
CONFIGURATION="Release"
VERSION_SUFFIX=""
OUTPUT_PATH="./artifacts"

# Parse arguments
while [[ $# -gt 0 ]]; do
  case $1 in
    --configuration|-c)
      CONFIGURATION="$2"
      shift 2
      ;;
    --version-suffix|-v)
      VERSION_SUFFIX="$2"
      shift 2
      ;;
    --output|-o)
      OUTPUT_PATH="$2"
      shift 2
      ;;
    *)
      echo "Unknown option: $1"
      exit 1
      ;;
  esac
done

# Create output directory if it doesn't exist
mkdir -p "$OUTPUT_PATH"

# Clean
echo -e "\e[32mCleaning solution...\e[0m"
dotnet clean ./src/webmock.sln -c "$CONFIGURATION"
if [ $? -ne 0 ]; then exit $?; fi

# Restore packages
echo -e "\e[32mRestoring packages...\e[0m"
dotnet restore ./src/webmock.sln
if [ $? -ne 0 ]; then exit $?; fi

# Build
echo -e "\e[32mBuilding solution...\e[0m"
dotnet build ./src/webmock.sln -c "$CONFIGURATION" --no-restore
if [ $? -ne 0 ]; then exit $?; fi

# Run tests
echo -e "\e[32mRunning tests...\e[0m"
dotnet test ./src/webmock.sln -c "$CONFIGURATION" --no-build
if [ $? -ne 0 ]; then exit $?; fi

# Create NuGet package
echo -e "\e[32mCreating NuGet package...\e[0m"
PACK_ARGS=("pack" "./src/Ave.WebMock/Ave.WebMock.csproj" "-c" "$CONFIGURATION" "--no-build" "-o" "$OUTPUT_PATH")

if [ -n "$VERSION_SUFFIX" ]; then
  PACK_ARGS+=("--version-suffix" "$VERSION_SUFFIX")
fi

dotnet "${PACK_ARGS[@]}"
if [ $? -ne 0 ]; then exit $?; fi

echo -e "\e[32mNuGet package created successfully in $OUTPUT_PATH\e[0m"