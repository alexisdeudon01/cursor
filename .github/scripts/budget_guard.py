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
