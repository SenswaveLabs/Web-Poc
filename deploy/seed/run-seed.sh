#!/bin/sh
# Runs the backend seed once. The seed host keeps running after seeding, so stop it when it logs that it finished.
# A marker in the seed-state volume skips the seed on later starts; `docker compose down -v` removes it together with the database.
set -u

MARKER=/seed-state/seeded
LOG=/tmp/seed.log

if [ -f "$MARKER" ]; then
    echo "Seed already done, skipping. Run 'docker compose down -v' to reset the database and seed again."
    exit 0
fi

dotnet Senswave.Presentation.Seed.dll > "$LOG" 2>&1 &
PID=$!
tail -f "$LOG" &
TAIL=$!

for _ in $(seq 1 180); do
    if grep -q "Initial seed finished\." "$LOG"; then
        kill "$PID" "$TAIL" 2>/dev/null
        touch "$MARKER"
        exit 0
    fi
    if ! kill -0 "$PID" 2>/dev/null; then
        kill "$TAIL" 2>/dev/null
        echo "Seed process stopped before it finished."
        exit 1
    fi
    sleep 1
done

kill "$PID" "$TAIL" 2>/dev/null
echo "Seed did not finish in 180 seconds."
exit 1
