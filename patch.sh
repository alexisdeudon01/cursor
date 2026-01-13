#!/usr/bin/env bash
set -euo pipefail

REPO="${1:-/home/tor/wkspaces/mo2}"
cd "$REPO"
echo "📌 Repo: $REPO"

test -f .github/workflows/auto-improve.yml || { echo "❌ Missing .github/workflows/auto-improve.yml"; exit 1; }

mkdir -p .github/scripts .github/budget

STAMP="$(date +%Y%m%d-%H%M%S)"
mkdir -p ".patch-backups/$STAMP"
cp .github/workflows/auto-improve.yml ".patch-backups/$STAMP/auto-improve.yml.bak"

# --------------------------
# 1) Create generate-uml-diagrams.py if missing
# --------------------------
if [ ! -f .github/scripts/generate-uml-diagrams.py ]; then
  echo "🧩 Creating .github/scripts/generate-uml-diagrams.py ..."
  cat > .github/scripts/generate-uml-diagrams.py <<'PY'
#!/usr/bin/env python3
from __future__ import annotations
from pathlib import Path
from typing import Dict, List
import sys

DIAGRAMS_DIR = Path(".cursor/agents/diagrams")
DIAGRAMS_DIR.mkdir(parents=True, exist_ok=True)

def write(name: str, content: str) -> Path:
    p = DIAGRAMS_DIR / name
    p.write_text(content, encoding="utf-8")
    return p

def arch(version:int)->Dict:
    m = f"""```mermaid
flowchart TB
  subgraph Build
    GH[GitHub Actions] --> CI[auto-improve.yml]
    CI --> DGM[generate-uml-diagrams.py]
    CI --> AI[auto-improve-ai.py / auto-improve.py]
    CI --> UB[unity-builder (docker)]
  end

  subgraph Runtime
    C[Client (NGO)] <--> S[Dedicated Server (authoritative)]
    C -->|RPC| NGO[Netcode for GameObjects]
    S -->|RPC| NGO
  end

  UB -->|Artifacts| C
  UB -->|Artifacts| S
```"""
    return {"type":"architecture","mermaid":write(f"architecture-v{version}.mmd", m), "desc":"Architecture CI + runtime (NGO)"}

def component(version:int)->Dict:
    m = f"""```mermaid
flowchart LR
  Client[Client App] --> NM[NetworkManager]
  Server[Dedicated Server] --> NM
  NM --> UT[UnityTransport]
  NM --> RPC[ServerRpc/ClientRpc]
  RPC --> AUTH[Authoritative Game Logic]
  AUTH --> STATE[Replicated State]
  STATE --> Client
```"""
    return {"type":"component","mermaid":write(f"component-v{version}.mmd", m), "desc":"Composants NGO + logique autoritaire"}

def sequence(version:int)->Dict:
    m = f"""```mermaid
sequenceDiagram
  participant Client
  participant NM as NGO/NetworkManager
  participant Server
  participant Game as GameLogic

  Client->>NM: StartClient()
  NM->>Server: Connect
  Server-->>NM: Accept + Spawn
  Server->>Game: Init authoritative state
  Server-->>Client: ClientRpc initial snapshot
  Client->>Client: Load scene / init UI
```"""
    return {"type":"sequence","mermaid":write(f"sequence-v{version}.mmd", m), "desc":"Séquence de connexion NGO"}

def fsm(version:int)->Dict:
    m = f"""```mermaid
stateDiagram-v2
  [*] --> Boot
  Boot --> MainMenu
  MainMenu --> Connecting: start client/server
  Connecting --> InLobby: connected
  InLobby --> InGame: start match
  InGame --> InLobby: end match
  InLobby --> MainMenu: disconnect
  Connecting --> MainMenu: fail/timeout
  InGame --> [*]: quit
```"""
    return {"type":"fsm","mermaid":write(f"fsm-v{version}.mmd", m), "desc":"FSM session"}

def er(version:int)->Dict:
    m = f"""```mermaid
erDiagram
  PLAYER ||--o{{ SESSION : joins
  SESSION ||--|| GAME : runs
  SESSION ||--o{{ ENTITY : has

  PLAYER {{
    string playerId
    string displayName
  }}
  SESSION {{
    string sessionId
    string mapId
  }}
  GAME {{
    string gameId
    string ruleset
  }}
  ENTITY {{
    string netId
    string prefab
  }}
```"""
    return {"type":"er","mermaid":write(f"er-v{version}.mmd", m), "desc":"ER logique"}

def links(version:int)->Dict:
    m = f"""```mermaid
graph LR
  Core --> NetworkingShared
  NetworkingShared --> NetworkingClient
  NetworkingShared --> NetworkingServer
  NetworkingServer --> DedicatedServer
  NetworkingClient --> ClientApp
```"""
    return {"type":"links","mermaid":write(f"links-v{version}.mmd", m), "desc":"Liens assemblies"}

def class_diag(version:int)->Dict:
    m = f"""```mermaid
classDiagram
  class NetworkManager
  class UnityTransport
  class ServerGame
  class ClientGame
  class ReplicatedState

  NetworkManager --> UnityTransport
  ServerGame --> ReplicatedState : writes
  ClientGame --> ReplicatedState : reads
```"""
    return {"type":"class","mermaid":write(f"class-v{version}.mmd", m), "desc":"Diagramme de classes (haut niveau)"}

def generate_all(version:int)->List[Dict]:
    return [
        arch(version),
        component(version),
        sequence(version),
        fsm(version),
        er(version),
        links(version),
        class_diag(version),
    ]

def main():
    version = int(sys.argv[1]) if len(sys.argv) > 1 else 1
    diags = generate_all(version)
    summary = DIAGRAMS_DIR / f"diagrams-v{version}.md"
    lines = [f"# Diagrams v{version}", ""]
    for d in diags:
        lines.append(f"## {d['type']}")
        lines.append(f"- {d['desc']}")
        lines.append(f"- Mermaid: `{d['mermaid']}`")
        lines.append("")
    summary.write_text("\n".join(lines), encoding="utf-8")
    print(f"✅ {len(diags)} diagramme(s) généré(s)")
    print(f"📄 Résumé: {summary}")

if __name__ == "__main__":
    main()
PY
  chmod +x .github/scripts/generate-uml-diagrams.py
fi

# --------------------------
# 2) Create budget_guard.py if missing
# --------------------------
if [ ! -f .github/scripts/budget_guard.py ]; then
  echo "🧩 Creating .github/scripts/budget_guard.py ..."
  cat > .github/scripts/budget_guard.py <<'PY'
#!/usr/bin/env python3
import json, os, time
from pathlib import Path
from datetime import datetime

BUDGET_EUR = float(os.getenv("BUDGET_EUR", "200"))
COST_PER_MIN_EUR = float(os.getenv("COST_PER_MIN_EUR", "0.01"))  # estimation
BUDGET_FILE = Path(os.getenv("BUDGET_FILE", ".github/budget/actions-budget.json"))
OUT = os.getenv("GITHUB_OUTPUT")
SUM = os.getenv("GITHUB_STEP_SUMMARY")

def load():
    if BUDGET_FILE.exists():
        return json.loads(BUDGET_FILE.read_text(encoding="utf-8"))
    return {"month": None, "total_eur": 0.0, "runs": []}

def save(d):
    BUDGET_FILE.parent.mkdir(parents=True, exist_ok=True)
    BUDGET_FILE.write_text(json.dumps(d, indent=2), encoding="utf-8")

def out(k,v):
    if OUT:
        with open(OUT, "a", encoding="utf-8") as f:
            f.write(f"{k}={v}\n")

def summ(lines):
    if SUM:
        with open(SUM, "a", encoding="utf-8") as f:
            f.write("\n".join(lines) + "\n")

def main():
    now = datetime.utcnow()
    month = now.strftime("%Y-%m")
    d = load()
    if d.get("month") != month:
        d = {"month": month, "total_eur": 0.0, "runs": []}

    phase = os.getenv("PHASE", "pre")
    if phase == "pre":
        skip = d["total_eur"] >= BUDGET_EUR
        out("budget_skip_heavy", "true" if skip else "false")
        out("budget_total_eur", f"{d['total_eur']:.2f}")
        summ([
            "## 💸 Budget guard (pré-run)",
            f"- Mois: **{month}**",
            f"- Total actuel: **{d['total_eur']:.2f} €** / **{BUDGET_EUR:.2f} €**",
            f"- Heavy steps: **{'SKIP' if skip else 'RUN'}**",
        ])
        return

    start_ts = float(os.getenv("BUDGET_START_TS", "0") or "0")
    dur_min = max(0.0, (time.time() - start_ts) / 60.0)
    cost = round(dur_min * COST_PER_MIN_EUR, 2)

    entry = {
        "ts_utc": now.isoformat()+"Z",
        "run_id": os.getenv("GITHUB_RUN_ID",""),
        "ref": os.getenv("GITHUB_REF",""),
        "duration_min": round(dur_min,2),
        "cost_eur": cost,
    }
    d["runs"].append(entry)
    d["total_eur"] = round(float(d["total_eur"]) + cost, 2)
    save(d)

    over = d["total_eur"] >= BUDGET_EUR
    out("budget_over", "true" if over else "false")
    out("budget_total_eur", f"{d['total_eur']:.2f}")
    summ([
        "## 💸 Budget guard (post-run)",
        f"- Durée: **{entry['duration_min']} min**",
        f"- Coût estimé run: **{entry['cost_eur']} €** (rate={COST_PER_MIN_EUR} €/min)",
        f"- Total mois: **{d['total_eur']:.2f} €** / **{BUDGET_EUR:.2f} €**",
        f"- Statut: **{'DÉPASSÉ' if over else 'OK'}**",
    ])

if __name__ == "__main__":
    main()
PY
  chmod +x .github/scripts/budget_guard.py
fi

# --------------------------
# 3) Patch workflow: single workflow, no parallel, fix AI block, add budget, add diagrams
# --------------------------
echo "🧩 Patching .github/workflows/auto-improve.yml ..."
python3 - <<'PY'
from pathlib import Path
import re

wf = Path(".github/workflows/auto-improve.yml")
y = wf.read_text(encoding="utf-8")

# A) remove push trigger block (keep schedule + workflow_dispatch)
y = re.sub(r"\n\s*push:\n(?:\s+.*\n)+", "\n", y, count=1)

# B) enforce schedule every 15 min (leave if already ok)
if "cron:" in y:
    y = re.sub(r"cron:\s*'[^']+'", "cron: '*/15 * * * *'", y)

# C) enforce global concurrency, cancel-in-progress true
if "concurrency:" in y:
    y = re.sub(r"concurrency:\n(?:\s+.*\n)+?(?=\n)", "concurrency:\n  group: auto-improve-global\n  cancel-in-progress: true\n", y, count=1)
else:
    y = re.sub(r"(on:\n(?:.*\n)+?)\n", r"\1\nconcurrency:\n  group: auto-improve-global\n  cancel-in-progress: true\n\n", y, count=1)

# D) Fix AI block: remove nested duplicated if and use one clean if
# (We replace any run block that contains two identical if lines)
y = re.sub(
    r"- name: Run Auto-Improve Script with AI[\s\S]*?run:\s*\|[\s\S]*?(?=\n\s*- name:)",
    "- name: Run Auto-Improve Script with AI\n"
    "        if: steps.budget_pre.outputs.budget_skip_heavy != 'true'\n"
    "        run: |\n"
    "          if [ -n \"${ANTHROPIC_API_KEY:-}\" ]; then\n"
    "            echo \"🤖 Utilisation de l'IA Claude pour amélioration...\"\n"
    "            python3 .github/scripts/auto-improve-ai.py\n"
    "          else\n"
    "            echo \"⚠️ Mode basique (pas d'IA)\"\n"
    "            python3 .github/scripts/auto-improve.py\n"
    "          fi\n",
    y
)

# E) Insert budget pre step after setup-python (best-effort)
if "Budget Guard (pre)" not in y:
    y = re.sub(
        r"(- name: Setup Python[\s\S]*?python-version:\s*'3\.11'\s*\n)",
        r"\1\n"
        "      - name: Budget Guard (pre)\n"
        "        id: budget_pre\n"
        "        run: |\n"
        "          echo \"BUDGET_START_TS=$(date +%s)\" >> $GITHUB_ENV\n"
        "          PHASE=pre python3 .github/scripts/budget_guard.py\n"
        "        env:\n"
        "          BUDGET_EUR: 200\n"
        "          COST_PER_MIN_EUR: 0.01\n",
        y,
        count=1
    )

# F) Ensure diagram step exists (uses our generator)
if "Generate UML Diagrams" not in y:
    y += (
        "\n      - name: Generate UML Diagrams\n"
        "        run: |\n"
        "          VERSION=$(date +%Y%m%d%H%M)\n"
        "          python3 .github/scripts/generate-uml-diagrams.py 1\n"
    )
else:
    # Replace existing diagram step body with ours (avoid mmdc)
    y = re.sub(
        r"- name: Generate UML Diagrams[\s\S]*?(?=\n\s*- name:|\Z)",
        "- name: Generate UML Diagrams\n"
        "        run: |\n"
        "          python3 .github/scripts/generate-uml-diagrams.py 1\n",
        y
    )

# G) Add budget post before commit (always)
if "Budget Guard (post)" not in y:
    y = re.sub(
        r"(\n\s*- name: Commit and Push Changes)",
        "\n      - name: Budget Guard (post)\n"
        "        if: always()\n"
        "        run: |\n"
        "          PHASE=post BUDGET_START_TS=${BUDGET_START_TS:-0} python3 .github/scripts/budget_guard.py\n"
        "        env:\n"
        "          BUDGET_EUR: 200\n"
        "          COST_PER_MIN_EUR: 0.01\n"
        r"\1",
        y,
        count=1
    )

wf.write_text(y, encoding="utf-8")
print("✅ Workflow patché")
PY

# --------------------------
# 4) Commit changes
# --------------------------
if [ -d .git ]; then
  git add -A
  if git diff --staged --quiet; then
    echo "ℹ️ Rien à commit."
  else
    git commit -m "ci: single workflow, global concurrency, budget guard, diagrams; fix AI block"
    echo "✅ Commit ok."
  fi
else
  echo "⚠️ Pas de .git -> skip commit"
fi

echo ""
echo "✅ Patch terminé."

# --------------------------
# 5) Trigger workflow (optional)
# --------------------------
if command -v gh >/dev/null 2>&1; then
  echo "🚀 Lancement workflow via gh (si le nom existe)..."
  gh workflow list || true
  echo "➡️ Si ton workflow s'appelle différemment, lance manuellement avec gh workflow run <name>"
else
  echo "ℹ️ gh CLI absent. Pour lancer le workflow: GitHub → Actions → Run workflow."
fi
