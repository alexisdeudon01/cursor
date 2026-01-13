#!/usr/bin/env python3
"""Cost Tracking for Auto-Improve Workflow"""

import json
import os
from datetime import datetime
from pathlib import Path

COSTS_FILE = Path(".github/config/costs.json")
REPORTS_DIR = Path(".github/reports")

def load_costs():
    if COSTS_FILE.exists():
        with open(COSTS_FILE) as f:
            return json.load(f)
    return {
        "pricing": {
            "anthropic": {"input_per_1k": 0.003, "output_per_1k": 0.015},
            "exchange_rate": 0.92
        },
        "totals": {
            "input_tokens": 0, "output_tokens": 0,
            "anthropic_usd": 0, "github_minutes": 0,
            "total_usd": 0, "total_eur": 0, "runs": 0,
            "first_run": None, "last_run": None
        },
        "history": []
    }

def save_costs(data):
    COSTS_FILE.parent.mkdir(exist_ok=True)
    with open(COSTS_FILE, 'w') as f:
        json.dump(data, f, indent=2)

def update_costs(input_tokens=0, output_tokens=0, minutes=0):
    data = load_costs()
    p = data["pricing"]
    t = data["totals"]
    
    cost = (input_tokens/1000)*p["anthropic"]["input_per_1k"] + (output_tokens/1000)*p["anthropic"]["output_per_1k"]
    
    t["input_tokens"] += input_tokens
    t["output_tokens"] += output_tokens
    t["anthropic_usd"] += cost
    t["github_minutes"] += minutes
    t["total_usd"] = t["anthropic_usd"]
    t["total_eur"] = t["total_usd"] * p["exchange_rate"]
    t["runs"] += 1
    
    now = datetime.utcnow().isoformat()
    if not t["first_run"]: t["first_run"] = now
    t["last_run"] = now
    
    data["history"].append({"time": now, "cost": round(cost, 6)})
    if len(data["history"]) > 50: data["history"] = data["history"][-50:]
    
    save_costs(data)

def print_summary():
    data = load_costs()
    t = data["totals"]
    print("=" * 50)
    print("💰 COÛTS TOTAUX")
    print("=" * 50)
    print(f"Runs: {t['runs']}")
    print(f"Tokens: {t['input_tokens']:,} in / {t['output_tokens']:,} out")
    print(f"Anthropic: ${t['anthropic_usd']:.4f}")
    print(f"GitHub: {t['github_minutes']:.1f} min (gratuit)")
    print("-" * 50)
    print(f"💵 USD: ${t['total_usd']:.4f}")
    print(f"💶 EUR: €{t['total_eur']:.4f}")
    print("=" * 50)

def generate_report():
    data = load_costs()
    t = data["totals"]
    REPORTS_DIR.mkdir(exist_ok=True)
    report = f"""# 💰 Rapport Coûts

| Métrique | Valeur |
|----------|--------|
| Runs | {t['runs']} |
| Tokens | {t['input_tokens']:,} / {t['output_tokens']:,} |
| **USD** | **${t['total_usd']:.4f}** |
| **EUR** | **€{t['total_eur']:.4f}** |
"""
    (REPORTS_DIR / "cost_report.md").write_text(report)

if __name__ == "__main__":
    import sys
    if len(sys.argv) > 1:
        if sys.argv[1] == "update" and len(sys.argv) >= 4:
            update_costs(int(sys.argv[2]), int(sys.argv[3]), float(sys.argv[4]) if len(sys.argv) > 4 else 0)
        elif sys.argv[1] == "report":
            generate_report()
    print_summary()
