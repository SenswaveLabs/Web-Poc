# Demo recording

Records the course demo video of Senswave against the deployed app. Each step gets a caption on screen.
The test runs a headed Chromium on a virtual display in Docker, and ffmpeg records the screen,
so the address bar and tabs are visible.

This is not a test suite. It is not part of `tests/e2e` runs or CI.

**It deletes data.** Before it starts, it removes all homes (with rooms and devices), shared homes and data sources
of both demo accounts. The accounts themselves stay.

## Run

```sh
cd tests/e2e/demo
cp .env.example .env   # fill in accounts and the MQTT broker
npm ci
./record.sh --dry-run  # fast headless run, screenshot per step in out/steps
./record.sh            # full recording
```

Requires Docker. The script builds a small image from the Playwright image with ffmpeg added.

## Output (`out/`)

| File | Content |
| --- | --- |
| `senswave-demo.mp4` | The recording, 1920×1080 |
| `chapters.txt` | Time of each caption, for the checklist |
| `lighthouse.html`, `lighthouse-scores.json` | Lighthouse report shown at the end |
| `failure-tab*.png`, `failure-tab*.txt` | Screenshot and accessibility tree of each tab when a step fails |

## Notes

- Sessions without "Remember me" live only in the tab, so the script never reloads the page after sign in.
  Two tabs can be signed in as two users at the same time.
- Lighthouse reloads the page, so user 1 signs in again with "Remember me" before the audit,
  and the audit keeps browser storage (`disableStorageReset`).
- `DEMO_PACE` scales every pause (default `0.8`). The dry run uses `0.25`.
