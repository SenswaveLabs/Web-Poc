#!/bin/sh
# Generates wwwroot/appsettings.json from container environment on every start,
# so API address and Google client id are configured in docker compose .env instead of baked into the image.
set -eu

: "${API_BASE_ADDRESS:?API_BASE_ADDRESS is not set}"
: "${GOOGLE_CLIENT_ID:=}"

HTML_ROOT=/usr/share/nginx/html

cat > "$HTML_ROOT/appsettings.json" <<EOF
{
  "Api": {
    "BaseUrl": "${API_BASE_ADDRESS}"
  },
  "Auth": {
    "Google": {
      "ClientId": "${GOOGLE_CLIENT_ID}"
    }
  }
}
EOF

# Remove precompressed copies of the build-time file so they can never be served instead.
rm -f "$HTML_ROOT/appsettings.json.br" "$HTML_ROOT/appsettings.json.gz"
