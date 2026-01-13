#!/usr/bin/env bash
set -euo pipefail

# Patch Unity asmdef platform issue:
# - Remove invalid "WindowsStandalone64Server" everywhere
# - Ensure "LinuxStandalone64Server" is present in server includePlatforms and client excludePlatforms
# - If any C# tool re-injects the invalid platform, patch it too (auto-discovery)

DEFAULT_REPO="/home/tor/wkspaces/mo2"
REPO="${1:-$DEFAULT_REPO}"

echo "📁 Repo: $REPO"
cd "$REPO"

command -v python3 >/dev/null || { echo "❌ python3 not found"; exit 1; }
command -v grep >/dev/null || { echo "❌ grep not found"; exit 1; }

ts="$(date +%Y%m%d_%H%M%S)"

# --- 1) Patch all *.asmdef that mention WindowsStandalone64Server (safe JSON edit) ---
echo "🔎 Searching asmdef files referencing WindowsStandalone64Server..."
mapfile -t ASMDEFS < <(grep -RIl --include="*.asmdef" 'WindowsStandalone64Server' . || true)

if [[ ${#ASMDEFS[@]} -eq 0 ]]; then
  echo "ℹ️ No .asmdef contains WindowsStandalone64Server. (Nothing to remove)"
else
  echo "✅ Found ${#ASMDEFS[@]} asmdef(s). Backing up + patching..."
  for f in "${ASMDEFS[@]}"; do
    cp -a "$f" "$f.bak.$ts"
  done

  # Remove invalid platform from includePlatforms/excludePlatforms arrays everywhere
  python3 - <<'PY'
import json
from pathlib import Path
import sys

def load(path: Path):
    return json.loads(path.read_text(encoding="utf-8"))

def save(path: Path, data):
    path.write_text(json.dumps(data, indent=4, ensure_ascii=False) + "\n", encoding="utf-8")

def remove_item(arr, item):
    if not isinstance(arr, list):
        return False
    before = list(arr)
    arr[:] = [x for x in arr if x != item]
    return arr != before

changed_any = False
paths = []
# read file list from grep results stored in a temp file is harder; just re-scan quickly:
for p in Path(".").rglob("*.asmdef"):
    try:
        if "WindowsStandalone64Server" not in p.read_text(encoding="utf-8"):
            continue
        paths.append(p)
    except Exception:
        continue

for p in paths:
    try:
        data = load(p)
    except Exception as e:
        print(f"⚠️ Skip (invalid JSON?): {p} ({e})")
        continue

    changed = False
    for key in ("includePlatforms", "excludePlatforms"):
        if key in data:
            changed |= remove_item(data[key], "WindowsStandalone64Server")

    if changed:
        save(p, data)
        changed_any = True
        print(f"✅ Patched: {p}")

if not changed_any:
    print("ℹ️ No asmdef JSON arrays needed changes (token may be elsewhere).")
PY
fi

# --- 2) Enforce the intended server/client platform rules for your known asmdefs if they exist ---
SERVER_ASMDEF="Assets/Scripts/Networking/Server/Server.asmdef"
CLIENT_ASMDEF="Assets/Scripts/Networking/Client/Client.asmdef"

python3 - <<PY
import json
from pathlib import Path

def load(path: Path):
    return json.loads(path.read_text(encoding="utf-8"))

def save(path: Path, data):
    path.write_text(json.dumps(data, indent=4, ensure_ascii=False) + "\\n", encoding="utf-8")

def ensure(arr, item):
    if not isinstance(arr, list):
        return False
    if item not in arr:
        arr.append(item)
        return True
    return False

def remove(arr, item):
    if not isinstance(arr, list):
        return False
    before = list(arr)
    arr[:] = [x for x in arr if x != item]
    return arr != before

changed = False

sp = Path("$SERVER_ASMDEF")
if sp.exists():
    s = load(sp)
    s.setdefault("includePlatforms", [])
    changed |= remove(s["includePlatforms"], "WindowsStandalone64Server")
    changed |= ensure(s["includePlatforms"], "LinuxStandalone64Server")
    save(sp, s)
    print(f"✅ Enforced server includePlatforms: {sp}")
else:
    print(f"ℹ️ Server asmdef not found: {sp}")

cp = Path("$CLIENT_ASMDEF")
if cp.exists():
    c = load(cp)
    c.setdefault("excludePlatforms", [])
    changed |= remove(c["excludePlatforms"], "WindowsStandalone64Server")
    changed |= ensure(c["excludePlatforms"], "LinuxStandalone64Server")
    save(cp, c)
    print(f"✅ Enforced client excludePlatforms: {cp}")
else:
    print(f"ℹ️ Client asmdef not found: {cp}")
PY

# --- 3) Patch any C# file that re-injects the invalid platform (auto-discovery) ---
echo
echo "🔎 Searching for C# files containing WindowsStandalone64Server (auto-patch if it's an injection list)..."
mapfile -t CSFILES < <(grep -RIl --include="*.cs" 'WindowsStandalone64Server' . || true)

if [[ ${#CSFILES[@]} -eq 0 ]]; then
  echo "ℹ️ No .cs file contains WindowsStandalone64Server. (Nothing to patch)"
else
  echo "✅ Found ${#CSFILES[@]} .cs file(s). Backing up + patching likely injection patterns..."
  for f in "${CSFILES[@]}"; do
    cp -a "$f" "$f.bak.$ts"

    # 1) Replace explicit pair "LinuxStandalone64Server", "WindowsStandalone64Server" -> "LinuxStandalone64Server"
    perl -0777 -i -pe 's/"LinuxStandalone64Server"\s*,\s*"WindowsStandalone64Server"/"LinuxStandalone64Server"/g' "$f"
    perl -0777 -i -pe 's/"WindowsStandalone64Server"\s*,\s*"LinuxStandalone64Server"/"LinuxStandalone64Server"/g' "$f"

    # 2) If there are standalone mentions in comma-separated lists, remove the invalid token cleanly
    perl -0777 -i -pe 's/\s*"WindowsStandalone64Server"\s*,\s*//g; s/,\s*"WindowsStandalone64Server"\s*//g; s/"WindowsStandalone64Server"\s*//g' "$f"
  done

  echo "✅ Patched C# files (removed WindowsStandalone64Server tokens)"
fi

# --- 4) Quick verification summary ---
echo
echo "🔍 Verification (should be empty):"
grep -RIn --include="*.asmdef" --include="*.cs" 'WindowsStandalone64Server' . || echo "✅ No remaining WindowsStandalone64Server occurrences."

echo
echo "🧾 Git diff (if git repo):"
if command -v git >/dev/null && git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
  git diff || true
else
  echo "ℹ️ Not a git repo (or git unavailable)."
fi

echo
echo "✅ Done."
