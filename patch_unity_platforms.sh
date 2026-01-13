#!/usr/bin/env bash
set -euo pipefail

REPO="${1:-/home/tor/wkspaces/mo2}"
cd "$REPO"

echo "📌 Repo: $REPO"

# --- 0) sanity
if [ ! -d ".git" ]; then
  echo "⚠️ Pas un repo git (ou .git absent). Je continue quand même."
fi

# --- 1) backup light (optionnel mais safe)
STAMP="$(date +%Y%m%d-%H%M%S)"
mkdir -p ".patch-backups/$STAMP"

echo "🧩 1) Patch ProjectTools.cs (stop re-inject WindowsStandalone64Server) ..."
PT="Assets/Editor/ProjectTools.cs"
if [ -f "$PT" ]; then
  cp "$PT" ".patch-backups/$STAMP/ProjectTools.cs.bak"

  python3 - <<'PY'
from pathlib import Path
p = Path("Assets/Editor/ProjectTools.cs")
s = p.read_text(encoding="utf-8", errors="ignore")

# Cas exact observé dans ton output:
s2 = s.replace(
    'content = content.Replace("\\"Server\\"", "\\"LinuxStandalone64Server\\", \\"WindowsStandalone64Server\\"");',
    'content = content.Replace("\\"Server\\"", "\\"LinuxStandalone64Server\\"");'
)

# Si jamais il existe d'autres variantes (plus robustes):
s2 = s2.replace("WindowsStandalone64Server", "")

# nettoyages basiques: doubles virgules dans les listes de chaînes, etc.
while ', ,' in s2:
    s2 = s2.replace(', ,', ', ')
s2 = s2.replace('", "', '", "').replace('""', '"')

if s2 != s:
    p.write_text(s2, encoding="utf-8")
    print("✅ ProjectTools.cs patché")
else:
    print("ℹ️ ProjectTools.cs: aucun changement nécessaire")
PY
else
  echo "ℹ️ $PT introuvable -> skip"
fi

echo "🧩 2) Patch tous les .asmdef (retirer WindowsStandalone64Server des listes) ..."
python3 - <<'PY'
import json
from pathlib import Path

def clean_list(lst):
    if not isinstance(lst, list):
        return lst
    return [x for x in lst if x != "WindowsStandalone64Server" and x != ""]

changed = 0
for f in Path("Assets").rglob("*.asmdef"):
    raw = f.read_text(encoding="utf-8", errors="ignore").strip()
    if not raw:
        continue
    try:
        data = json.loads(raw)
    except Exception:
        continue

    before = raw
    data["includePlatforms"] = clean_list(data.get("includePlatforms", []))
    data["excludePlatforms"] = clean_list(data.get("excludePlatforms", []))

    # si jamais un champ custom contient le token
    dumped = json.dumps(data, indent=2, ensure_ascii=False)
    dumped = dumped.replace("WindowsStandalone64Server", "")
    dumped = dumped.replace(",\n  ]", "\n  ]").replace('",\n  ]', '"\n  ]')

    if dumped.strip() != before.strip():
        f.write_text(dumped + "\n", encoding="utf-8")
        changed += 1

print(f"✅ asmdef patchés: {changed}")
PY

echo "🧩 3) Patch global (strings/code) : supprimer token WindowsStandalone64Server ..."
# (On exclut Library/ et Build/ et .git/ pour éviter le bruit)
python3 - <<'PY'
from pathlib import Path

EXCLUDE_DIRS = {"Library", "Build", ".git", "Logs", "Temp", "obj", "PackagesCache"}
changed = 0

def is_excluded(p: Path) -> bool:
    parts = set(p.parts)
    return any(x in parts for x in EXCLUDE_DIRS)

for p in Path(".").rglob("*"):
    if p.is_dir() or is_excluded(p):
        continue
    if p.suffix.lower() in {".cs", ".json", ".yml", ".yaml", ".md", ".txt", ".asmdef", ".asmref"}:
        try:
            s = p.read_text(encoding="utf-8", errors="ignore")
        except Exception:
            continue
        if "WindowsStandalone64Server" in s:
            s2 = s.replace("WindowsStandalone64Server", "")
            # nettoyages simples
            s2 = s2.replace(", ,", ", ").replace('"",', '"').replace('""', '"')
            if s2 != s:
                p.write_text(s2, encoding="utf-8")
                changed += 1

print(f"✅ fichiers patchés (token global): {changed}")
PY

echo "🔍 4) Vérification finale (doit être vide) ..."
set +e
grep -R --line-number "WindowsStandalone64Server" \
  Assets .github .cursor 2>/dev/null
RC=$?
set -e

if [ "$RC" -eq 0 ]; then
  echo "❌ Encore des occurrences. Regarde la sortie ci-dessus."
  exit 2
else
  echo "✅ OK: plus aucune occurrence de WindowsStandalone64Server"
fi

echo "🧾 5) Résumé git diff (si repo git) ..."
if [ -d ".git" ]; then
  git status --porcelain || true
  git diff --stat || true
fi

echo "✅ Patch terminé."
