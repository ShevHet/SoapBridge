#!/usr/bin/env bash
set -e
# Find the first package.json (search up to depth 6)
PKG_PATH=$(find . -maxdepth 6 -name package.json | head -n1 || true)
if [ -z "$PKG_PATH" ]; then
  echo "No package.json found in repo (searched depth 6). Skipping npm script addition."
  exit 0
fi
echo "Found package.json at: $PKG_PATH"
# Use node to safely edit JSON
node -e '
const fs = require("fs");
const p = process.argv[1];
const pkg = JSON.parse(fs.readFileSync(p, "utf8"));
if (!pkg.scripts) pkg.scripts = {};
let modified = false;
if (!pkg.scripts.start) {
  if (pkg.main) {
    pkg.scripts.start = "node " + pkg.main;
  } else if (pkg.scripts.dev) {
    pkg.scripts.start = "npm run dev";
  } else if (pkg.scripts.build) {
    pkg.scripts.start = "npm run build";
  } else {
    pkg.scripts.start = "node index.js";
  }
  console.log("Added start script -> " + pkg.scripts.start);
  modified = true;
}
if (!pkg.scripts.build) {
  // Heuristic: add common build commands when dependencies indicate framework
  const deps = Object.assign({}, pkg.dependencies, pkg.devDependencies);
  if (deps && deps["react-scripts"]) {
    pkg.scripts.build = "react-scripts build";
  } else if (deps && deps["vite"]) {
    pkg.scripts.build = "vite build";
  } else if (deps && deps["next"]) {
    pkg.scripts.build = "next build";
  } else {
    // do not add a generic build if we cannot confidently guess
  }
  if (pkg.scripts.build) {
    console.log("Ensured build script -> " + pkg.scripts.build);
    modified = true;
  }
}
if (modified) {
  fs.writeFileSync(p, JSON.stringify(pkg, null, 2) + "\n");
}
' "$PKG_PATH"

# Make the script executable (in repo)
chmod +x .github/scripts/ensure-npm-scripts.sh || true
