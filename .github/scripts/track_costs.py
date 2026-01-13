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
    return None

def save_costs(data):
    with open(COSTS_FILE, 'w') as f:
        json.dump(data, f, indent=2)

def update_costs(input_tokens=0, output_tokens=0, minutes=0):
    data = load_costs()
    if not data:
        print("❌ costs.json not found")
        return
    
    pricing = data["pricing"]
    totals = data["totals"]
    
    # Anthropic costs
    anthropic_cost = (
        (input_tokens / 1000) * pricing["anthropic"]["claude-sonnet-4"]["input_per_1k_tokens"] +
        (output_tokens / 1000) * pricing["anthropic"]["claude-sonnet-4"]["output_per_1k_tokens"]
    )
    
    # GitHub costs (free for public)
    github_cost = minutes * pricing["github_actions"]["ubuntu_latest_per_minute"]
    
    # Update totals
    totals["anthropic_input_tokens"] += input_tokens
    totals["anthropic_output_tokens"] += output_tokens
    totals["anthropic_cost_usd"] += anthropic_cost
    totals["github_minutes"] += minutes
    totals["github_cost_usd"] += github_cost
    totals["total_cost_usd"] = totals["anthropic_cost_usd"] + totals["github_cost_usd"]
    totals["total_cost_eur"] = totals["total_cost_usd"] * pricing["exchange_rate"]["USD_to_EUR"]
    totals["total_runs"] += 1
    
    now = datetime.utcnow().isoformat()
    if not totals["first_run"]:
        totals["first_run"] = now
    totals["last_run"] = now
    
    # History
    data["history"].append({
        "timestamp": now,
        "run_number": os.environ.get("GITHUB_RUN_NUMBER", "local"),
        "input_tokens": input_tokens,
        "output_tokens": output_tokens,
        "cost_usd": round(anthropic_cost + github_cost, 6)
    })
    if len(data["history"]) > 100:
        data["history"] = data["history"][-100:]
    
    save_costs(data)
    return data

def print_summary():
    data = load_costs()
    if not data:
        return
    
    t = data["totals"]
    print("=" * 60)
    print("💰 RÉSUMÉ DES COÛTS")
    print("=" * 60)
    print(f"📊 Total Runs: {t['total_runs']}")
    print(f"🤖 Anthropic: {t['anthropic_input_tokens']:,} in / {t['anthropic_output_tokens']:,} out")
    print(f"   Coût API: ${t['anthropic_cost_usd']:.4f}")
    print(f"⚙️  GitHub: {t['github_minutes']:.1f} min (gratuit repo public)")
    print("-" * 60)
    print(f"💵 TOTAL USD: ${t['total_cost_usd']:.4f}")
    print(f"💶 TOTAL EUR: €{t['total_cost_eur']:.4f}")
    print("=" * 60)

def generate_report():
    data = load_costs()
    if not data:
        return
    
    t = data["totals"]
    REPORTS_DIR.mkdir(exist_ok=True)
    
    report = f"""# 💰 Rapport des Coûts

**Mis à jour:** {datetime.utcnow().strftime('%Y-%m-%d %H:%M')} UTC

## Résumé

| Métrique | Valeur |
|----------|--------|
| Total Runs | {t['total_runs']} |
| Premier run | {t['first_run'] or 'N/A'} |
| Dernier run | {t['last_run'] or 'N/A'} |

## Coûts par composant

| Composant | Détail | Coût USD |
|-----------|--------|----------|
| Anthropic | {t['anthropic_input_tokens']:,} in + {t['anthropic_output_tokens']:,} out | ${t['anthropic_cost_usd']:.4f} |
| GitHub Actions | {t['github_minutes']:.1f} min | $0.00 (gratuit) |

## Total

| 💵 USD | 💶 EUR |
|--------|--------|
| ${t['total_cost_usd']:.4f} | €{t['total_cost_eur']:.4f} |
"""
    
    with open(REPORTS_DIR / "cost_report.md", 'w') as f:
        f.write(report)
    print("✅ Rapport: .github/reports/cost_report.md")

if __name__ == "__main__":
    import sys
    if len(sys.argv) > 1:
        cmd = sys.argv[1]
        if cmd == "update" and len(sys.argv) >= 4:
            update_costs(int(sys.argv[2]), int(sys.argv[3]), float(sys.argv[4]) if len(sys.argv) > 4 else 0)
        elif cmd == "report":
            generate_report()
    print_summary()
