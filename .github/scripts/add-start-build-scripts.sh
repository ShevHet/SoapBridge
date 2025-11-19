#!/usr/bin/env bash

set -e

# Find the first package.json (search up to depth 6)
PKG_PATH=$(find . -maxdepth 6 -name package.json | head -n1 || true)

if [ -z "$PKG_PATH" ]; then
  echo "No package.json found in repo (searched depth 6). Skipping start/build script addition."
  exit 0
fi

echo "Found package.json at: $PKG_PATH"

# Use node to safely edit JSON
node -e '
const fs = require("fs");
const p = process.argv[1];
const pkg = JSON.parse(fs.readFileSync(p, "utf8") || "{}");
pkg.scripts = pkg.scripts || {};
let changed = false;

// Add start script if missing
if (!pkg.scripts.start) {
  if (pkg.main) {
    pkg.scripts.start = "node " + pkg.main;
  } else if (pkg.scripts.dev) {
    pkg.scripts.start = "npm run dev";
  } else if (pkg.scripts.serve) {
    pkg.scripts.start = "npm run serve";
  } else {
    // Default fallback
    pkg.scripts.start = "node index.js";
  }
  console.log("Added start script -> " + pkg.scripts.start);
  changed = true;
} else {
  console.log("start script already present -> " + pkg.scripts.start);
}

// Add build script heuristics if missing
if (!pkg.scripts.build) {
  const deps = Object.assign({}, pkg.dependencies, pkg.devDependencies);
  if (deps["react-scripts"]) {
    pkg.scripts.build = "react-scripts build";
  } else if (deps["next"]) {
    pkg.scripts.build = "next build";
  } else if (deps["vite"]) {
    pkg.scripts.build = "vite build";
  } else if (deps["@angular/cli"]) {
    pkg.scripts.build = "ng build --prod";
  } else if (pkg.scripts.dev) {
    // Conservative: make build call dev or print notice
    pkg.scripts.build = "echo \"No build step defined. If you have a frontend, set scripts.build in package.json\"";
  } else {
    pkg.scripts.build = "echo \"No build step defined. If you have a frontend, set scripts.build in package.json\"";
  }
  console.log("Added build script -> " + pkg.scripts.build);
  changed = true;
} else {
  console.log("build script already present -> " + pkg.scripts.build);
}

if (changed) {
  fs.writeFileSync(p, JSON.stringify(pkg, null, 2) + "\n");
  process.exit(0);
} else {
  process.exit(0);
}
' "$PKG_PATH"

# Ensure executable bit for the script in repo
chmod +x .github/scripts/add-start-build-scripts.sh || true

