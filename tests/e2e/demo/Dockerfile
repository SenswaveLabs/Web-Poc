# Headed Chromium on a virtual display, recorded with ffmpeg.
FROM mcr.microsoft.com/playwright:v1.64.1-noble
RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg fonts-noto-color-emoji \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /demo
