#!/usr/bin/env bash

set -e

# Find the first package.json (search up to depth 6)
PKG_PATH=$(find . -maxdepth 6 -name package.json | head -n1 || true)

if [ -z "$PKG_PATH" ]; then
  echo "No package.json found in repo (searched depth 6). Skipping start script addition."
  exit 0
fi

echo "Found package.json at: $PKG_PATH"

# Use node to safely edit JSON (keeps formatting minimal)
node -e '
const fs = require("fs");
const p = process.argv[1];
const pkg = JSON.parse(fs.readFileSync(p, "utf8"));
if (!pkg.scripts) pkg.scripts = {};
if (!pkg.scripts.start) {
  if (pkg.main) {
    pkg.scripts.start = "node " + pkg.main;
  } else if (pkg.scripts.dev) {
    pkg.scripts.start = "npm run dev";
  } else if (pkg.scripts.build) {
    pkg.scripts.start = "npm run build && node dist/index.js";
  } else {
    pkg.scripts.start = "node index.js";
  }
  fs.writeFileSync(p, JSON.stringify(pkg, null, 2) + "\n");
  console.log("Added start script to " + p + " -> " + pkg.scripts.start);
} else {
  console.log("start script already present in " + p + " -> " + pkg.scripts.start);
}
' "$PKG_PATH"

# Make the script executable (when added to repo)
chmod +x .github/scripts/add-start-script.sh || true
