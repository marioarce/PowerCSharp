#!/usr/bin/env bash
set -euo pipefail

# Packs the PowerCSharp Features packages into a specified target directory so the template can
# consume them via the "powercsharp-local" NuGet source (see NuGet.Config).
#
# Usage: scripts/pack-local-feed.sh --target <target-directory> [--source <source-repo>]
#   --target: Required. Target directory where .nupkg files will be copied.
#   --source: Optional. Path to PowerCSharp repo. Defaults to current repo root.

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SRC="$REPO_ROOT"
FEED=""

# Parse arguments
while [[ $# -gt 0 ]]; do
  case $1 in
    --target)
      FEED="$2"
      shift 2
      ;;
    --source)
      SRC="$2"
      shift 2
      ;;
    *)
      echo "Unknown option: $1" >&2
      echo "Usage: scripts/pack-local-feed.sh --target <target-directory> [--source <source-repo>]" >&2
      exit 1
      ;;
  esac
done

# Validate required arguments
if [ -z "$FEED" ]; then
  echo "Error: --target parameter is required" >&2
  echo "Usage: scripts/pack-local-feed.sh --target <target-directory> [--source <source-repo>]" >&2
  exit 1
fi

PROJECTS=(
  "src/PowerCSharp.Core"
  "src/PowerCSharp.Extensions"
  "src/PowerCSharp.Helpers"
  "src/Features/PowerCSharp.Features.Abstractions"
  "src/Features/PowerCSharp.Features"
  "src/Features/PowerCSharp.BuiltInFeatures"
  "src/Features/PowerCSharp.Feature.Cache.Abstractions"
  "src/Features/PowerCSharp.Feature.Cache"
  "src/Features/PowerCSharp.Feature.Cache.BitFaster"
  "src/Features/PowerCSharp.Feature.Cache.Disk"
)

if [ ! -d "$SRC" ]; then
  echo "PowerCSharp repo not found at: $SRC" >&2
  echo "Pass the path explicitly: scripts/pack-local-feed.sh --target <target> --source /path/to/PowerCSharp" >&2
  exit 1
fi

mkdir -p "$FEED"

# Remove old .nupkg files from target folder to ensure only current versions are present
rm -f "$FEED"/*.nupkg

# Build projects first to ensure dependencies are up-to-date
for p in "${PROJECTS[@]}"; do
  dotnet build "$SRC/$p" -c Release --nologo
done

# Pack projects explicitly to generate .nupkg files
for p in "${PROJECTS[@]}"; do
  dotnet pack "$SRC/$p" -c Release --no-build --nologo
done

# Copy generated packages to the target folder
for p in "${PROJECTS[@]}"; do
  cp "$SRC/$p"/bin/Release/*.nupkg "$FEED/"
done

echo "Packed $(ls -1 "$FEED"/*.nupkg | wc -l | tr -d ' ') packages into $FEED"
