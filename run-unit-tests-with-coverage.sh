#!/bin/bash

# Exit on error
set -e

# Install reportgenerator if not already installed
if ! command -v reportgenerator &> /dev/null
then
    echo "Installing reportgenerator..."
    dotnet tool install --global dotnet-reportgenerator-globaltool
fi

# Clean up previous results
rm -rf ./coveragereport
rm -rf ./dotnet-etcd.Tests/TestResults

# Run unit tests with coverage
echo "Running unit tests with coverage..."
dotnet test dotnet-etcd.Tests/dotnet-etcd.Tests.csproj --filter "Category=Unit" --collect:"XPlat Code Coverage" --results-directory ./coverage

# Generate report excluding gRPC generated code
echo "Generating coverage report..."
reportgenerator \
  -reports:"./coverage/**/coverage.cobertura.xml" \
  -targetdir:"./coveragereport" \
  -reporttypes:Html,TextSummary \
  -classfilters:"-Authpb.*;-Etcdserverpb.*;-Mvccpb.*;-V3Electionpb.*;-V3Lockpb.*;-Versionpb.*;-Gogoproto.*"

# Display summary
echo "Coverage report generated at ./coveragereport/index.html"
echo ""
echo "Coverage Summary:"
cat ./coveragereport/Summary.txt

# Open the report if on macOS or Windows
if [[ "$OSTYPE" == "darwin"* ]]; then
    open ./coveragereport/index.html
elif [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" ]]; then
    start ./coveragereport/index.html
fi 