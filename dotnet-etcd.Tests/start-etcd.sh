#!/bin/bash

# Default to single node if no argument is provided
CLUSTER_TYPE=${1:-single}

# Stop any running etcd containers
echo "Stopping any running etcd containers..."
docker stop etcd etcd1 etcd2 etcd3 2>/dev/null || true
docker rm etcd etcd1 etcd2 etcd3 2>/dev/null || true

# Start the requested cluster type
if [ "$CLUSTER_TYPE" == "3nodes" ]; then
    echo "Starting 3-node etcd cluster..."
    cd "$(dirname "$0")"
    docker compose -f docker-compose-3nodes.yml up -d
    
    # Wait for the cluster to be ready
    echo "Waiting for the cluster to be ready..."
    sleep 5
    
    # Check if the cluster is healthy
    echo "Checking cluster health..."
    docker exec etcd1 etcdctl --endpoints=http://etcd1:2379,http://etcd2:2379,http://etcd3:2379 endpoint health
    
    echo "3-node etcd cluster is running."
    echo "Endpoints:"
    echo "  - http://localhost:2379 (etcd1)"
    echo "  - http://localhost:22379 (etcd2)"
    echo "  - http://localhost:32379 (etcd3)"
    
    # Create a file to indicate which cluster is running
    echo "3nodes" > "$(dirname "$0")/cluster-type.txt"
else
    echo "Starting single-node etcd cluster..."
    cd "$(dirname "$0")"
    docker compose up -d
    
    # Wait for the cluster to be ready
    echo "Waiting for the cluster to be ready..."
    sleep 3
    
    # Check if the cluster is healthy
    echo "Checking cluster health..."
    docker exec etcd etcdctl --endpoints=http://0.0.0.0:2379 endpoint health
    
    echo "Single-node etcd cluster is running."
    echo "Endpoint: http://localhost:2379"
    
    # Create a file to indicate which cluster is running
    echo "single" > "$(dirname "$0")/cluster-type.txt"
fi 