#!/usr/bin/env python3
"""
Phase 3: Évalue les changements et décide du rollback
"""
import os
import json
import requests
from pathlib import Path

API_KEY = os.environ.get("ANTHROPIC_API_KEY")

def call_claude(prompt):
    if not API_KEY:
        return None
    
    try:
        response = requests.post(
            "https://api.anthropic.com/v1/messages",
            headers={
                "x-api-key": API_KEY,
                "anthropic-version": "2023-06-01",
                "content-type": "application/json"
            },
            json={
                "model": "claude-sonnet-4-20250514",
                "max_tokens": 2048,
                "messages": [{"role": "user", "content": prompt}]
            },
            timeout=60
        )
        response.raise_for_status()
        return response.json()["content"][0]["text"]
    except:
        return None

def load_previous_score():
    """Charge le score précédent"""
    history_file = Path(".github/reports/score_history.json")
    if history_file.exists():
        history = json.loads(history_file.read_text())
        if history:
            return history[-1].get("score", 50)
    return 50

def save_score(score):
    """Sauvegarde le score dans l'historique"""
    history_file = Path(".github/reports/score_history.json")
    history = []
    if history_file.exists():
        try:
            history = json.loads(history_file.read_text())
        except:
            pass
    
    from datetime import datetime
    history.append({
        "timestamp": datetime.now().isoformat(),
        "score": score
    })
    
    # Garder les 100 derniers
    history = history[-100:]
    history_file.write_text(json.dumps(history, indent=2))

def main():
    print("="*60)
    print("📊 PHASE 3: ÉVALUATION DES CHANGEMENTS")
    print("="*60)
    
    # Charger métriques actuelles
    metrics_file = Path(".github/reports/current_metrics.json")
    if metrics_file.exists():
        metrics = json.loads(metrics_file.read_text())
        score_after = metrics.get("total_score", 50)
    else:
        score_after = 50
    
    score_before = load_previous_score()
    
    print(f"📈 Score avant: {score_before}")
    print(f"📈 Score après: {score_after}")
    
    # Décision rollback
    should_rollback = score_after < score_before - 5  # Tolérance de 5 points
    
    if should_rollback:
        print(f"⚠️ RÉGRESSION: {score_before} → {score_after} (diff: {score_after - score_before})")
        print("   → ROLLBACK recommandé")
    else:
        print(f"✅ Amélioration ou stable: {score_before} → {score_after}")
        save_score(score_after)
    
    # Output pour GitHub Actions
    github_output = os.environ.get("GITHUB_OUTPUT", "/dev/null")
    with open(github_output, "a") as f:
        f.write(f"score_before={score_before}\n")
        f.write(f"score_after={score_after}\n")
        f.write(f"should_rollback={'true' if should_rollback else 'false'}\n")
    
    print(f"\n{'❌ ROLLBACK' if should_rollback else '✅ COMMIT'}")

if __name__ == "__main__":
    main()
