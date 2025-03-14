#!/bin/bash

# Default to single node if no argument is provided
CLUSTER_TYPE=${1:-single}

# Start the etcd cluster
echo "Starting etcd cluster..."
./start-etcd.sh $CLUSTER_TYPE

# Run the integration tests
echo "Running integration tests..."
dotnet test --filter Category=Integration

# Get the test result
TEST_RESULT=$?

# Stop the etcd cluster
echo "Stopping etcd cluster..."
./stop-etcd.sh

# Exit with the test result
exit $TEST_RESULT 