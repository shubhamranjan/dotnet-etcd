#!/bin/bash
set -euo pipefail

cd "$(dirname "$0")"

echo "Stopping etcd test containers..."
docker compose down

rm -f cluster-type.txt

echo "etcd test containers stopped."
