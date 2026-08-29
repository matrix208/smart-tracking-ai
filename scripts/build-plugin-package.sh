#!/bin/zsh
set -e

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PLUGIN="$ROOT/src/Plugins/Tracking.Plugin.GT06"
OUT="$ROOT/repository/GT06/package"

echo "============================================================"
echo "BUILD GT06 PLUGIN PACKAGE"
echo "============================================================"

rm -rf "$OUT"
mkdir -p "$OUT/Manifest"

dotnet build \
  "$PLUGIN/Tracking.Plugin.GT06.csproj" \
  --no-restore

cp "$PLUGIN/bin/Debug/net10.0/Tracking.Plugin.GT06.dll" \
   "$OUT/Tracking.Plugin.GT06.dll"

for dll in \
  Tracking.SDK.dll \
  Tracking.Core.dll \
  Tracking.Network.dll \
  Tracking.Protocol.dll \
  Tracking.Events.dll \
  Tracking.Persistence.dll \
  Tracking.Storage.dll
do
  find "$ROOT/src" \
    -type f \
    -name "$dll" \
    -path "*/bin/Debug/net10.0/*" \
    -print -quit \
    -exec cp {} "$OUT/$dll" \;
done

cp "$PLUGIN/Manifest/manifest.json" \
   "$OUT/Manifest/manifest.json"

echo
echo "Package created:"
find "$OUT" -maxdepth 2 -type f -print | sort
