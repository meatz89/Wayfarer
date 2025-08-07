#!/bin/bash
cd /mnt/c/git/wayfarer/src
timeout 5 dotnet run --no-build 2>&1 | head -200