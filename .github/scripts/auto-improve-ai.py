#!/usr/bin/env python3
"""
Auto-Improve AI Script
Analyse le projet et propose des améliorations via Claude API
"""

import os
import json
import requests
from pathlib import Path
from datetime import datetime

ANTHROPIC_API_KEY = os.environ.get("ANTHROPIC_API_KEY")
API_URL = "https://api.anthropic.com/v1/messages"

def get_project_files():
    """Récupère les fichiers du projet pour analyse"""
    files = {}
    
    # Scripts C#
    for cs_file in Path("Assets/Scripts").rglob("*.cs"):
        try:
            files[str(cs_file)] = cs_file.read_text()[:2000]
        except:
            pass
    
    # Agent file
    agent_file = Path(".cursor/agents/thebestclient.md")
    if agent_file.exists():
        files[str(agent_file)] = agent_file.read_text()
    
    return files

def call_claude(prompt):
    """Appelle Claude API"""
    if not ANTHROPIC_API_KEY:
        print("❌ ANTHROPIC_API_KEY non définie")
        return None
    
    headers = {
        "x-api-key": ANTHROPIC_API_KEY,
        "anthropic-version": "2023-06-01",
        "content-type": "application/json"
    }
    
    data = {
        "model": "claude-sonnet-4-20250514",
        "max_tokens": 4096,
        "messages": [{"role": "user", "content": prompt}]
    }
    
    try:
        response = requests.post(API_URL, headers=headers, json=data, timeout=60)
        response.raise_for_status()
        return response.json()["content"][0]["text"]
    except Exception as e:
        print(f"❌ Erreur API: {e}")
        return None

def main():
    print("🤖 Auto-Improve AI Starting...")
    print(f"📅 {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    
    # Récupérer les fichiers
    files = get_project_files()
    print(f"📁 {len(files)} fichiers analysés")
    
    if not files:
        print("⚠️ Aucun fichier à analyser")
        return
    
    # Construire le prompt
    files_summary = "\n".join([f"- {f}" for f in files.keys()])
    
    prompt = f"""Tu es un agent d'amélioration de code pour un projet Unity.

Projet: TheBestClient (Full Authoritative Server, DOD)

Fichiers présents:
{files_summary}

Instructions:
1. Analyse la structure actuelle
2. Vérifie la conformité DOD (structs pour DTOs)
3. Vérifie l'architecture Full Authoritative Server
4. Propose des améliorations concrètes

Réponds avec:
- État actuel (OK/À améliorer)
- Actions suggérées
- Code à ajouter/modifier (si nécessaire)
"""

    print("🧠 Analyse en cours...")
    result = call_claude(prompt)
    
    if result:
        print("\n" + "="*60)
        print("📊 RÉSULTAT ANALYSE")
        print("="*60)
        print(result[:2000])
        
        # Sauvegarder le rapport
        report_dir = Path(".github/reports")
        report_dir.mkdir(exist_ok=True)
        report_file = report_dir / f"report-{datetime.now().strftime('%Y%m%d-%H%M%S')}.md"
        report_file.write_text(f"# Rapport Auto-Improve\n\n{result}")
        print(f"\n📄 Rapport sauvegardé: {report_file}")
    else:
        print("⚠️ Pas de résultat de l'API")

if __name__ == "__main__":
    main()
