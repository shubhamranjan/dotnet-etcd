#!/bin/bash
set -euo pipefail

# The test topology lives in a single docker-compose.yml:
#   - etcd1 / etcd2 / etcd3 : 3-node cluster   (ports 2379 / 22379 / 32379)
#   - etcd-ssl              : TLS-enabled node (port 2389)
#   - etcd-authttl          : short auth-token-ttl node for token-renewal tests (port 2399)
cd "$(dirname "$0")"

# The fixture reads cluster-type.txt to pick its connection string. The cluster is reachable on
# 2379 regardless, so default to "single"; pass "3nodes" to exercise all three endpoints.
CLUSTER_TYPE=${1:-single}

# Generate the TLS certificates the etcd-ssl node mounts. They are gitignored, so generate them
# if missing.
if [ ! -f certs/ca.pem ]; then
    echo "Generating TLS certificates..."
    chmod +x generate-certs.sh
    ./generate-certs.sh
fi

echo "Starting etcd test containers..."
docker compose up -d

# Poll until a container reports healthy. docker compose up -d returns as soon as containers are
# created, not when etcd is actually serving, so we must wait before running tests against them.
wait_for_health() {
    local name="$1"
    shift
    echo "Waiting for ${name} to be ready..."
    for _ in $(seq 1 30); do
        if docker exec "${name}" etcdctl "$@" endpoint health >/dev/null 2>&1; then
            echo "${name} is healthy."
            return 0
        fi
        sleep 2
    done
    echo "ERROR: ${name} did not become healthy within 60s." >&2
    docker compose ps >&2 || true
    return 1
}

# The 3-node cluster (most integration tests) and the short-TTL auth node (token-renewal test).
wait_for_health etcd1 --endpoints=http://etcd1:2379,http://etcd2:2379,http://etcd3:2379
wait_for_health etcd-authttl

echo "$CLUSTER_TYPE" > cluster-type.txt

echo "etcd test containers are running:"
echo "  - http://localhost:2379    (etcd1)"
echo "  - http://localhost:22379   (etcd2)"
echo "  - http://localhost:32379   (etcd3)"
echo "  - https://localhost:2389   (etcd-ssl)"
echo "  - http://localhost:2399    (etcd-authttl, auth-token-ttl=2s)"
