#!/bin/sh
# Records the Senswave demo to out/senswave-demo.mp4 (with out/chapters.txt for the checklist).
# Usage: ./record.sh            full recording
#        ./record.sh --dry-run  fast headless run, no video
set -eu
cd "$(dirname "$0")"

[ -f .env ] || { echo "Missing .env. Copy .env.example to .env and fill it in." >&2; exit 1; }
[ -d node_modules ] || npm ci

docker build -q -t senswave-demo-recorder . >/dev/null

if [ "${1:-}" = "--dry-run" ]; then
  RUN_ENV="-e DEMO_HEADLESS=1 -e DEMO_PACE=0.25 -e DEMO_STEP_SHOTS=1"
  CMD="npx playwright test"
else
  RUN_ENV="-e DEMO_RECORD=1"
  CMD="xvfb-run --auto-servernum --server-args='-screen 0 1920x1080x24' npx playwright test"
fi

docker run --rm --ipc=host --init \
  -u "$(id -u):$(id -g)" -e HOME=/tmp \
  $RUN_ENV \
  -v "$PWD:/demo" -w /demo \
  senswave-demo-recorder sh -c "$CMD"
