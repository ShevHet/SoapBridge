#!/usr/bin/env bash

set -e

# Find the first package.json (search up to depth 6)
PKG_PATH=$(find . -maxdepth 6 -name package.json | head -n1 || true)

if [ -z "$PKG_PATH" ]; then
  echo "No package.json found in repo (searched depth 6). Skipping start script addition."
  exit 0
fi

echo "Found package.json at: $PKG_PATH"

# Check if scripts.start already exists
if grep -q '"start"' "$PKG_PATH" 2>/dev/null; then
  echo "package.json already has a 'start' script. Skipping."
  exit 0
fi

# Determine the directory containing package.json
PKG_DIR=$(dirname "$PKG_PATH")

# Read package.json and check for heuristics
HAS_MAIN=$(grep -q '"main"' "$PKG_PATH" 2>/dev/null && echo "yes" || echo "no")
HAS_DEV=$(grep -q '"dev"' "$PKG_PATH" 2>/dev/null && echo "yes" || echo "no")
HAS_BUILD=$(grep -q '"build"' "$PKG_PATH" 2>/dev/null && echo "yes" || echo "no")

# Determine start script based on heuristics
START_SCRIPT=""
if [ "$HAS_MAIN" = "yes" ]; then
  MAIN_FILE=$(grep '"main"' "$PKG_PATH" | sed -E 's/.*"main"[[:space:]]*:[[:space:]]*"([^"]+)".*/\1/')
  START_SCRIPT="node $MAIN_FILE"
elif [ "$HAS_DEV" = "yes" ]; then
  START_SCRIPT="npm run dev"
elif [ "$HAS_BUILD" = "yes" ]; then
  START_SCRIPT="npm run build && node dist/index.js"
else
  START_SCRIPT="node index.js"
fi

echo "Adding start script: $START_SCRIPT"

# Use Node.js or Python to safely modify JSON
if command -v node >/dev/null 2>&1; then
  node -e "
    const fs = require('fs');
    const pkg = JSON.parse(fs.readFileSync('$PKG_PATH', 'utf8'));
    if (!pkg.scripts) pkg.scripts = {};
    if (!pkg.scripts.start) {
      pkg.scripts.start = '$START_SCRIPT';
      fs.writeFileSync('$PKG_PATH', JSON.stringify(pkg, null, 2) + '\n');
      console.log('Added start script to package.json');
    }
  "
elif command -v python3 >/dev/null 2>&1; then
  python3 -c "
import json
import sys
with open('$PKG_PATH', 'r') as f:
    pkg = json.load(f)
if 'scripts' not in pkg:
    pkg['scripts'] = {}
if 'start' not in pkg['scripts']:
    pkg['scripts']['start'] = '$START_SCRIPT'
    with open('$PKG_PATH', 'w') as f:
        json.dump(pkg, f, indent=2)
        f.write('\n')
    print('Added start script to package.json')
"
else
  echo "Error: Neither node nor python3 found. Cannot modify package.json safely."
  exit 1
fi

