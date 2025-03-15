#!/bin/bash

# Read the cluster type from the file
CLUSTER_TYPE_FILE="$(dirname "$0")/cluster-type.txt"
if [ -f "$CLUSTER_TYPE_FILE" ]; then
    CLUSTER_TYPE=$(cat "$CLUSTER_TYPE_FILE")
else
    CLUSTER_TYPE="single"
fi

# Stop the cluster based on the type
if [ "$CLUSTER_TYPE" == "3nodes" ]; then
    echo "Stopping 3-node etcd cluster..."
    cd "$(dirname "$0")"
    docker compose -f docker-compose-3nodes.yml down
else
    echo "Stopping single-node etcd cluster..."
    cd "$(dirname "$0")"
    docker compose down
fi

# Remove the cluster type file
rm -f "$CLUSTER_TYPE_FILE"

echo "etcd cluster stopped." 