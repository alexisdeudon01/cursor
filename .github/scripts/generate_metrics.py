#!/usr/bin/env python3
"""
Phase 1: Claude génère les métriques d'évaluation
"""
import os
import json
import requests
from pathlib import Path

API_KEY = os.environ.get("ANTHROPIC_API_KEY")
API_URL = "https://api.anthropic.com/v1/messages"

def get_project_content():
    """Récupère le contenu du projet"""
    content = {"scripts": {}, "agent": "", "structure": []}
    
    # Scripts
    for f in Path("Assets/Scripts").rglob("*.cs"):
        try:
            content["scripts"][str(f)] = f.read_text()[:3000]
        except:
            pass
    
    # Agent
    agent_path = Path(".cursor/agents/agent.md")
    if agent_path.exists():
        content["agent"] = agent_path.read_text()
    
    # Structure Unity
    for ext in ["*.unity", "*.meta", "*.prefab"]:
        for f in Path(".").rglob(ext):
            content["structure"].append(str(f))
    
    return content

def call_claude(prompt):
    """Appelle Claude API"""
    if not API_KEY:
        print("❌ ANTHROPIC_API_KEY non définie!")
        print("   Vérifiez que le secret est configuré sur GitHub")
        return None
    
    headers = {
        "x-api-key": API_KEY,
        "anthropic-version": "2023-06-01",
        "content-type": "application/json"
    }
    
    data = {
        "model": "claude-sonnet-4-20250514",
        "max_tokens": 4096,
        "messages": [{"role": "user", "content": prompt}]
    }
    
    try:
        print("📡 Appel Claude API (timeout: 120s)...")
        response = requests.post(API_URL, headers=headers, json=data, timeout=120)
        
        if response.status_code == 401:
            print("❌ Erreur 401: Clé API invalide ou expirée")
            print("   Vérifiez ANTHROPIC_API_KEY dans les secrets GitHub")
            return None
        
        if response.status_code == 429:
            print("⚠️ Rate limit atteint, attente 30s...")
            import time
            time.sleep(30)
            response = requests.post(API_URL, headers=headers, json=data, timeout=120)
        
        response.raise_for_status()
        return response.json()["content"][0]["text"]
    
    except requests.exceptions.Timeout:
        print("❌ Timeout API (120s dépassé)")
        print("   L'API Anthropic met trop de temps à répondre")
        return None
    
    except requests.exceptions.ConnectionError as e:
        print(f"❌ Erreur connexion: {e}")
        print("   Vérifiez la connectivité réseau")
        return None
    
    except requests.exceptions.RequestException as e:
        print(f"❌ Erreur API: {e}")
        return None

def main():
    print("="*60)
    print("📊 PHASE 1: GÉNÉRATION MÉTRIQUES PAR CLAUDE")
    print("="*60)
    
    content = get_project_content()
    print(f"📁 {len(content['scripts'])} scripts trouvés")
    print(f"📄 Agent: {'Oui' if content['agent'] else 'Non'}")
    print(f"🔧 {len(content['structure'])} fichiers Unity")
    
    prompt = f"""Analyse ce projet Unity et génère des métriques d'évaluation.

## AGENT (règles à suivre):
{content['agent'][:3000]}

## SCRIPTS ACTUELS:
{json.dumps(list(content['scripts'].keys()), indent=2)}

## CONTENU DES SCRIPTS:
{chr(10).join([f"=== {k} ===\n{v[:1000]}" for k,v in list(content['scripts'].items())[:5]])}

---

Génère un JSON avec:
1. Métriques spécifiques pour évaluer ce projet
2. Score actuel (0-100) pour chaque métrique
3. Score total pondéré
4. Améliorations prioritaires

Format EXACT (JSON valide):
{{
  "metrics": [
    {{"name": "Server Authority", "weight": 0.25, "score": X, "details": "Logique serveur uniquement, pas de StartHost"}},
    {{"name": "Single Build", "weight": 0.15, "score": X, "details": "Un seul exécutable, scènes séparées"}},
    {{"name": "UI Toolkit", "weight": 0.20, "score": X, "details": "UXML + USS, pas de Canvas legacy"}},
    {{"name": "Structure Discovery", "weight": 0.15, "score": X, "details": "Lecture .unity/.meta via GUIDs"}},
    {{"name": "Network Flow", "weight": 0.15, "score": X, "details": "Séquence connexion correcte"}},
    {{"name": "Build Ready", "weight": 0.10, "score": X, "details": "Compilation sans erreurs"}}
  ],
  "total_score": X,
  "improvements": ["...", "...", "..."]
}}

IMPORTANT: Réponds UNIQUEMENT avec le JSON, pas de texte avant ou après.
"""
    
    result = call_claude(prompt)
    
    if result:
        # Sauvegarder les métriques
        metrics_dir = Path(".github/reports")
        metrics_dir.mkdir(exist_ok=True)
        
        # Extraire JSON
        try:
            # Trouver le JSON dans la réponse
            start = result.find("{")
            end = result.rfind("}") + 1
            if start >= 0 and end > start:
                json_str = result[start:end]
                metrics = json.loads(json_str)
                
                metrics_file = metrics_dir / "current_metrics.json"
                metrics_file.write_text(json.dumps(metrics, indent=2))
                
                print(f"\n📊 Score total: {metrics.get('total_score', 'N/A')}")
                print(f"📄 Métriques sauvegardées: {metrics_file}")
                
                # Output pour GitHub Actions
                with open(os.environ.get("GITHUB_OUTPUT", "/dev/null"), "a") as f:
                    f.write(f"score={metrics.get('total_score', 0)}\n")
            else:
                print("⚠️ JSON non trouvé dans la réponse")
                print(result[:500])
        except json.JSONDecodeError as e:
            print(f"⚠️ Erreur parsing JSON: {e}")
            print(result[:500])
    else:
        print("⚠️ Pas de réponse de Claude")
        # Créer métriques par défaut
        default_metrics = {
            "metrics": [
                {"name": "Server Authority", "weight": 0.3, "score": 50, "details": "À évaluer"},
                {"name": "Structure Discovery", "weight": 0.2, "score": 50, "details": "À évaluer"},
                {"name": "Network Flow", "weight": 0.2, "score": 50, "details": "À évaluer"},
                {"name": "Build Ready", "weight": 0.2, "score": 50, "details": "À évaluer"},
                {"name": "Code Quality", "weight": 0.1, "score": 50, "details": "À évaluer"}
            ],
            "total_score": 50,
            "improvements": ["Vérifier clé API", "Ajouter scripts", "Configurer Unity"]
        }
        Path(".github/reports").mkdir(exist_ok=True)
        Path(".github/reports/current_metrics.json").write_text(json.dumps(default_metrics, indent=2))

if __name__ == "__main__":
    main()
