#!/usr/bin/env python3
"""
Cost Tracking for Auto-Improve Workflow
Tracks Anthropic API usage and GitHub Actions minutes
"""

import json
import os
from datetime import datetime
from pathlib import Path

COSTS_FILE = Path(".github/config/costs.json")
REPORTS_DIR = Path(".github/reports")

def load_costs():
    """Load existing costs or create new"""
    if COSTS_FILE.exists():
        with open(COSTS_FILE) as f:
            return json.load(f)
    return None

def save_costs(data):
    """Save costs to file"""
    with open(COSTS_FILE, 'w') as f:
        json.dump(data, f, indent=2)

def update_costs(input_tokens=0, output_tokens=0, minutes=0, model="claude-sonnet-4"):
    """Update cost tracking with new usage"""
    data = load_costs()
    if not data:
        print("❌ costs.json not found")
        return
    
    pricing = data["pricing"]
    totals = data["totals"]
    
    # Calculate Anthropic costs
    model_pricing = pricing["anthropic"].get(model, pricing["anthropic"]["claude-sonnet-4"])
    anthropic_cost = (
        (input_tokens / 1000) * model_pricing["input_per_1k_tokens"] +
        (output_tokens / 1000) * model_pricing["output_per_1k_tokens"]
    )
    
    # Calculate GitHub Actions costs (free for public repos, but track anyway)
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
    
    # Add to history
    run_entry = {
        "timestamp": now,
        "run_number": os.environ.get("GITHUB_RUN_NUMBER", "local"),
        "input_tokens": input_tokens,
        "output_tokens": output_tokens,
        "anthropic_cost_usd": round(anthropic_cost, 6),
        "github_minutes": minutes,
        "github_cost_usd": round(github_cost, 6),
        "total_cost_usd": round(anthropic_cost + github_cost, 6)
    }
    
    # Keep last 100 runs in history
    data["history"].append(run_entry)
    if len(data["history"]) > 100:
        data["history"] = data["history"][-100:]
    
    save_costs(data)
    
    return data

def print_summary():
    """Print cost summary"""
    data = load_costs()
    if not data:
        print("❌ No cost data found")
        return
    
    totals = data["totals"]
    
    print("╔════════════════════════════════════════════════════════════╗")
    print("║                    💰 RÉSUMÉ DES COÛTS                     ║")
    print("╠════════════════════════════════════════════════════════════╣")
    print(f"║  📊 Total Runs: {totals['total_runs']}")
    print(f"║  📅 Premier run: {totals['first_run'] or 'N/A'}")
    print(f"║  📅 Dernier run: {totals['last_run'] or 'N/A'}")
    print("╠════════════════════════════════════════════════════════════╣")
    print("║  🤖 ANTHROPIC API:")
    print(f"║     Input tokens:  {totals['anthropic_input_tokens']:,}")
    print(f"║     Output tokens: {totals['anthropic_output_tokens']:,}")
    print(f"║     Coût: ${totals['anthropic_cost_usd']:.4f} USD")
    print("╠════════════════════════════════════════════════════════════╣")
    print("║  ⚙️  GITHUB ACTIONS:")
    print(f"║     Minutes: {totals['github_minutes']:.1f}")
    print(f"║     Coût: ${totals['github_cost_usd']:.4f} USD (gratuit repo public)")
    print("╠════════════════════════════════════════════════════════════╣")
    print(f"║  💵 TOTAL USD: ${totals['total_cost_usd']:.4f}")
    print(f"║  💶 TOTAL EUR: €{totals['total_cost_eur']:.4f}")
    print("╚════════════════════════════════════════════════════════════╝")

def generate_cost_report():
    """Generate markdown cost report"""
    data = load_costs()
    if not data:
        return
    
    totals = data["totals"]
    REPORTS_DIR.mkdir(exist_ok=True)
    
    report = f"""# 💰 Rapport des Coûts - Auto-Improve

**Généré le:** {datetime.utcnow().strftime('%Y-%m-%d %H:%M:%S')} UTC

## 📊 Résumé Global

| Métrique | Valeur |
|----------|--------|
| Total Runs | {totals['total_runs']} |
| Premier run | {totals['first_run'] or 'N/A'} |
| Dernier run | {totals['last_run'] or 'N/A'} |

## 🤖 Anthropic API

| Composant | Valeur | Coût |
|-----------|--------|------|
| Input tokens | {totals['anthropic_input_tokens']:,} | - |
| Output tokens | {totals['anthropic_output_tokens']:,} | - |
| **Sous-total** | - | **${totals['anthropic_cost_usd']:.4f}** |

## ⚙️ GitHub Actions

| Composant | Valeur | Coût |
|-----------|--------|------|
| Minutes utilisées | {totals['github_minutes']:.1f} | ${totals['github_cost_usd']:.4f} |

> ℹ️ GitHub Actions est gratuit pour les repos publics

## 💰 Total

| Devise | Montant |
|--------|---------|
| **USD** | **${totals['total_cost_usd']:.4f}** |
| **EUR** | **€{totals['total_cost_eur']:.4f}** |

## 📈 Historique (10 derniers runs)

| Run | Date | Tokens (in/out) | Coût |
|-----|------|-----------------|------|
"""
    
    for entry in data["history"][-10:]:
        report += f"| {entry['run_number']} | {entry['timestamp'][:10]} | {entry['input_tokens']}/{entry['output_tokens']} | ${entry['total_cost_usd']:.4f} |\n"
    
    report_path = REPORTS_DIR / "cost_report.md"
    with open(report_path, 'w') as f:
        f.write(report)
    
    print(f"✅ Rapport généré: {report_path}")

if __name__ == "__main__":
    import sys
    
    if len(sys.argv) > 1:
        cmd = sys.argv[1]
        
        if cmd == "update":
            # Usage: track_costs.py update <input_tokens> <output_tokens> <minutes>
            input_t = int(sys.argv[2]) if len(sys.argv) > 2 else 0
            output_t = int(sys.argv[3]) if len(sys.argv) > 3 else 0
            mins = float(sys.argv[4]) if len(sys.argv) > 4 else 0
            update_costs(input_t, output_t, mins)
            print_summary()
            
        elif cmd == "summary":
            print_summary()
            
        elif cmd == "report":
            generate_cost_report()
            print_summary()
    else:
        print_summary()
