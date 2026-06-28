#!/bin/bash
#
# beast.sh — run the test suite repeatedly to flush out flaky / timing-dependent
# failures (races in the watch/auth/lease streaming paths won't show up in a
# single green run).
#
# Builds once, then runs the same binaries N times with --no-build so each
# iteration exercises identical code and only non-determinism can cause a
# failure.
#
# Usage:
#   ./beast.sh [-n iterations] [-f Unit|Integration|All] [-k]
#
#   -n   number of iterations (default: 20)
#   -f   test category filter: Unit, Integration, or All (default: All)
#   -k   keep going after a failure and report the totals
#        (default: stop on the first failing iteration)
#
# Integration/All runs require the etcd test containers; this script starts them
# via start-etcd.sh if 127.0.0.1:2379 is not reachable.
set -uo pipefail

cd "$(dirname "$0")"

ITERATIONS=20
FILTER="All"
KEEP_GOING=0

while getopts ":n:f:k" opt; do
    case "$opt" in
        n) ITERATIONS="$OPTARG" ;;
        f) FILTER="$OPTARG" ;;
        k) KEEP_GOING=1 ;;
        *) echo "Usage: $0 [-n iterations] [-f Unit|Integration|All] [-k]" >&2; exit 2 ;;
    esac
done

case "$FILTER" in
    Unit)        FILTER_ARG=(--filter "Category=Unit") ;;
    Integration) FILTER_ARG=(--filter "Category=Integration") ;;
    All)         FILTER_ARG=() ;;
    *) echo "Invalid -f '$FILTER' (expected Unit|Integration|All)" >&2; exit 2 ;;
esac

PROJECT="dotnet-etcd.Tests.csproj"
LOG_DIR="$(mktemp -d)"

# Integration tests need etcd; bring it up if it isn't already serving.
if [ "$FILTER" != "Unit" ]; then
    if ! curl -s --max-time 3 http://127.0.0.1:2379/version >/dev/null 2>&1; then
        echo "etcd not reachable — starting test containers..."
        ./start-etcd.sh
    fi
fi

echo "Building once (Debug)..."
dotnet build "$PROJECT" -c Debug >/dev/null || { echo "Build failed." >&2; exit 1; }

echo "Beasting: $ITERATIONS iteration(s), filter=$FILTER, keep-going=$KEEP_GOING"
echo "Logs: $LOG_DIR"

passes=0
failures=0
failed_iterations=()

for i in $(seq 1 "$ITERATIONS"); do
    log="$LOG_DIR/run-$i.log"
    if dotnet test "$PROJECT" -c Debug --no-build "${FILTER_ARG[@]}" >"$log" 2>&1; then
        passes=$((passes + 1))
        echo "  [$i/$ITERATIONS] PASS"
    else
        failures=$((failures + 1))
        failed_iterations+=("$i")
        echo "  [$i/$ITERATIONS] FAIL -> $log"
        grep -iE "\[FAIL\]|Failed " "$log" | head -10 | sed 's/^/      /'
        if [ "$KEEP_GOING" -eq 0 ]; then
            echo ""
            echo "Stopping on first failure (use -k to keep going). Full log: $log"
            exit 1
        fi
    fi
done

echo ""
echo "================ beast summary ================"
echo "  iterations : $ITERATIONS"
echo "  passed     : $passes"
echo "  failed     : $failures"
if [ "$failures" -gt 0 ]; then
    echo "  flaky on   : ${failed_iterations[*]}"
    echo "  logs       : $LOG_DIR"
    exit 1
fi
echo "  result     : all green — no flakiness detected"
echo "=============================================="
