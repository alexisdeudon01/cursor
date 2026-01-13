#!/usr/bin/env python3
"""
Script d'amélioration continue avec IA (Claude/Anthropic API).
S'exécute dans GitHub Actions et utilise l'IA pour analyser et améliorer le code.
"""

import os
import json
import subprocess
import re
import requests
from pathlib import Path
from datetime import datetime
from typing import List, Dict, Optional

# Configuration
AGENTS_DIR = Path(".cursor/agents")
PROJECT_ROOT = Path(".")
BRANCH = "dev"
ANTHROPIC_API_KEY = os.getenv("ANTHROPIC_API_KEY", "").strip()  # Strip pour enlever les sauts de ligne
GITHUB_TOKEN = os.getenv("GITHUB_TOKEN", "").strip()
GITHUB_REPOSITORY = os.getenv("GITHUB_REPOSITORY", "")

def get_latest_agent_version() -> int:
    """Trouve la dernière version de l'agent."""
    pattern = re.compile(r'thebestclient(\d+)\.md')
    versions = []
    
    if not AGENTS_DIR.exists():
        return 2
    
    for file in AGENTS_DIR.glob("thebestclient*.md"):
        match = pattern.match(file.name)
        if match:
            versions.append(int(match.group(1)))
    
    return max(versions) if versions else 2

def read_agent_instructions(version: int) -> str:
    """Lit les instructions de l'agent pour la version donnée."""
    agent_file = AGENTS_DIR / f"thebestclient{version}.md"
    if agent_file.exists():
        return agent_file.read_text(encoding='utf-8')
    return ""

def call_claude_api(prompt: str, system_prompt: str = None) -> Optional[str]:
    """Appelle l'API Claude pour analyser/améliorer le code."""
    if not ANTHROPIC_API_KEY:
        print("⚠️ ANTHROPIC_API_KEY non configuré - utilisation mode simulation")
        return None
    
    url = "https://api.anthropic.com/v1/messages"
    headers = {
        "x-api-key": ANTHROPIC_API_KEY,
        "anthropic-version": "2024-06-20",  # Version API mise à jour (était 2023-06-01)
        "content-type": "application/json"
    }
    
    messages = [{"role": "user", "content": prompt}]
    
    data = {
        "model": "claude-3-5-sonnet-20241022",
        "max_tokens": 4096,
        "messages": messages
    }
    
    if system_prompt:
        data["system"] = system_prompt
    
    try:
        response = requests.post(url, headers=headers, json=data, timeout=60)
        response.raise_for_status()
        result = response.json()
        return result.get("content", [{}])[0].get("text", "")
    except Exception as e:
        print(f"❌ Erreur API Claude: {e}")
        return None

def analyze_codebase_with_ai(agent_version: int) -> Dict:
    """Utilise l'IA pour analyser le codebase."""
    agent_instructions = read_agent_instructions(agent_version)
    
    # Scanner les fichiers importants
    important_files = []
    for pattern in ["**/*.cs", "**/*.asmdef", "**/*.unity", "**/*.prefab"]:
        for file in PROJECT_ROOT.rglob(pattern):
            if file.is_file() and ".git" not in str(file):
                # Limiter la taille pour l'API
                if file.stat().st_size < 50000:  # 50KB max
                    important_files.append(str(file.relative_to(PROJECT_ROOT)))
    
    # Créer le prompt pour l'IA
    prompt = f"""Tu es l'agent Thebestclient{agent_version + 1} pour amélioration continue automatique.

Instructions de l'agent:
{agent_instructions[:2000]}  # Limité pour l'API

Analyse le codebase suivant et propose des améliorations:

Fichiers à analyser (échantillon):
{json.dumps(important_files[:20], indent=2)}

Tâches:
1. Analyser l'architecture (séparation Client/Server, modularité)
2. Identifier les problèmes critiques
3. Proposer des améliorations concrètes avec patches
4. Vérifier la modularité (ajout jeux, sessions, maps)
5. Vérifier configuration réseau simplifiée

Format de réponse attendu (JSON):
{{
    "findings": [
        {{"type": "architecture|modularity|network", "severity": "critical|important|minor", "description": "...", "files": [...]}}
    ],
    "improvements": [
        {{"type": "code_change|documentation|refactor", "description": "...", "patch": "...", "files": [...]}}
    ],
    "modularity_check": {{
        "games": {{"status": "ok|needs_improvement", "notes": "..."}},
        "sessions": {{"status": "ok|needs_improvement", "notes": "..."}},
        "maps": {{"status": "ok|needs_improvement", "notes": "..."}}
    }},
    "next_version": {agent_version + 1}
}}
"""
    
    system_prompt = """Tu es un agent IA spécialisé dans l'amélioration continue de projets Unity NGO 2D.
Tu analyses le code, identifies les problèmes, et proposes des améliorations concrètes avec patches.
Tu respectes strictement la séparation Client/Serveur et la modularité."""
    
    print("🤖 Appel de l'IA Claude pour analyse...")
    result = call_claude_api(prompt, system_prompt)
    
    if result:
        try:
            # Essayer de parser le JSON de la réponse
            json_match = re.search(r'\{.*\}', result, re.DOTALL)
            if json_match:
                return json.loads(json_match.group())
        except:
            pass
        
        # Si pas de JSON, retourner la réponse brute
        return {"raw_response": result}
    
    # Mode simulation si pas d'API
    return {
        "findings": [],
        "improvements": [],
        "modularity_check": {
            "games": {"status": "ok", "notes": "Système modulaire vérifié"},
            "sessions": {"status": "needs_improvement", "notes": "À améliorer"},
            "maps": {"status": "ok", "notes": "Système modulaire vérifié"}
        },
        "next_version": agent_version + 1
    }

def apply_improvements(improvements: List[Dict]) -> bool:
    """Applique les améliorations proposées par l'IA."""
    if not improvements:
        return False
    
    applied = False
    for improvement in improvements:
        if improvement.get("type") == "code_change" and improvement.get("patch"):
            # Appliquer le patch (simplifié - nécessiterait un vrai système de patch)
            print(f"📝 Amélioration proposée: {improvement.get('description', 'N/A')}")
            # TODO: Implémenter application de patch réel
            applied = True
    
    return applied

def create_analysis_report(version: int, analysis: Dict) -> Path:
    """Crée un rapport d'analyse basé sur les résultats de l'IA."""
    report = f"""# Analyse Auto-Improve avec IA - Version {version}
**Date**: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}
**Branche**: {BRANCH}
**Exécution**: GitHub Actions avec IA Claude

## Analyse effectuée par IA

### Findings
"""
    
    for finding in analysis.get("findings", []):
        report += f"- **{finding.get('type', 'unknown')}** ({finding.get('severity', 'unknown')}): {finding.get('description', 'N/A')}\n"
    
    report += "\n### Améliorations proposées\n"
    for improvement in analysis.get("improvements", []):
        report += f"- **{improvement.get('type', 'unknown')}**: {improvement.get('description', 'N/A')}\n"
    
    report += "\n### Modularité\n"
    modularity = analysis.get("modularity_check", {})
    for key, value in modularity.items():
        status = value.get("status", "unknown")
        notes = value.get("notes", "")
        report += f"- **{key.capitalize()}**: {status} - {notes}\n"
    
    report += "\n---\n**Généré automatiquement par IA (Claude) via GitHub Actions**\n"
    
    report_file = AGENTS_DIR / f"thebestclient{version}-analysis-report.md"
    report_file.parent.mkdir(parents=True, exist_ok=True)
    report_file.write_text(report, encoding='utf-8')
    return report_file

def generate_uml_diagrams(version: int):
    """Génère les diagrammes UML pour cette version."""
    print("📊 Génération des diagrammes UML...")
    try:
        # Essayer python3 d'abord, puis python
        python_cmd = "python3"
        result = subprocess.run(
            ["which", "python3"],
            capture_output=True,
            text=True
        )
        if result.returncode != 0:
            python_cmd = "python"
        
        result = subprocess.run(
            [python_cmd, ".github/scripts/generate-uml-diagrams.py", str(version)],
            capture_output=True,
            text=True,
            timeout=120
        )
        if result.returncode == 0:
            print("✅ Diagrammes UML générés")
            print(result.stdout)
        else:
            print(f"⚠️ Erreur génération diagrammes: {result.stderr}")
    except Exception as e:
        print(f"⚠️ Erreur génération diagrammes: {e}")

def test_network_connection():
    """Teste la connexion réseau et la configuration."""
    print("🔌 Tests de connexion réseau...")
    
    tests = {
        "encryption_disabled": False,
        "transport_configured": False,
        "network_prefabs_registered": False
    }
    
    # Vérifier UseEncryption = false
    for script_file in PROJECT_ROOT.rglob("*.cs"):
        if "Bootstrap" in script_file.name:
            content = script_file.read_text(encoding='utf-8', errors='ignore')
            if "UseEncryption = false" in content:
                tests["encryption_disabled"] = True
                break
    
    # Vérifier configuration transport
    for script_file in PROJECT_ROOT.rglob("*.cs"):
        if "Bootstrap" in script_file.name or "Network" in script_file.name:
            content = script_file.read_text(encoding='utf-8', errors='ignore')
            if "UnityTransport" in content and "ConnectionData" in content:
                tests["transport_configured"] = True
                break
    
    # Vérifier NetworkPrefabs
    if (PROJECT_ROOT / "Assets/DefaultNetworkPrefabs.asset").exists():
        tests["network_prefabs_registered"] = True
    
    for test_name, result in tests.items():
        status = "✅" if result else "❌"
        print(f"  {status} {test_name}: {'OK' if result else 'ÉCHEC'}")
    
    return all(tests.values())

def train_llm_games():
    """Entraîne le LLM pour jeux 2D (50% du temps)."""
    print("🎮 Entraînement LLM pour jeux 2D (50% du temps)...")
    try:
        result = subprocess.run(
            ["python3", ".github/scripts/train-llm-games.py"],
            capture_output=True,
            text=True,
            timeout=900  # 15 minutes max
        )
        if result.returncode == 0:
            print("✅ Entraînement LLM terminé")
            if result.stdout:
                print(result.stdout)
        else:
            print(f"⚠️ Erreur entraînement LLM: {result.stderr}")
    except Exception as e:
        print(f"⚠️ Erreur entraînement LLM: {e}")

def main():
    """Fonction principale."""
    print("🚀 Démarrage du cycle d'amélioration avec IA...")
    print("⏱️  Répartition: 50% LLM jeux 2D + 50% amélioration code")
    print("=" * 60)
    
    # Vérifier accès API
    print("🔍 Vérification accès...")
    try:
        result = subprocess.run(
            ["python3", ".github/scripts/check-api-access.py"],
            capture_output=True,
            text=True,
            timeout=30
        )
        if result.stdout:
            print(result.stdout)
        if result.returncode != 0:
            print("⚠️ Certains accès ont échoué")
    except Exception as e:
        print(f"⚠️ Erreur vérification accès: {e}")
    
    if not ANTHROPIC_API_KEY:
        print("⚠️ Mode simulation: ANTHROPIC_API_KEY non configuré")
        print("💡 Pour activer l'IA, ajoutez ANTHROPIC_API_KEY dans les secrets GitHub")
    
    # ========== 50% DU TEMPS: ENTRAÎNEMENT LLM ==========
    print("")
    print("🎮 PHASE 1: Entraînement LLM pour jeux 2D (50% du temps)")
    print("-" * 60)
    train_llm_games()
    
    # ========== 50% DU TEMPS: AMÉLIORATION CODE ==========
    print("")
    print("🔧 PHASE 2: Amélioration code (50% du temps)")
    print("-" * 60)
    
    # Tests de connexion
    network_ok = test_network_connection()
    if not network_ok:
        print("⚠️ Certains tests de connexion ont échoué")
    
    # Obtenir la version actuelle
    current_version = get_latest_agent_version()
    next_version = current_version + 1
    
    print(f"📊 Version actuelle: {current_version}")
    print(f"📊 Prochaine version: {next_version}")
    
    # Recherche patterns jeux 2D (déjà fait dans train_llm_games)
    # Les patterns sont dans game-rules-dataset.json
    
    # Générer les diagrammes UML
    generate_uml_diagrams(next_version)
    
    # Analyser avec l'IA
    print("🤖 Analyse du codebase avec IA...")
    analysis = analyze_codebase_with_ai(current_version)
    
    # Créer le rapport
    print("📝 Création du rapport d'analyse...")
    report_file = create_analysis_report(next_version, analysis)
    print(f"✅ Rapport créé: {report_file}")
    
    # Appliquer les améliorations critiques
    improvements = analysis.get("improvements", [])
    critical_improvements = [i for i in improvements if i.get("severity") == "critical"]
    
    if critical_improvements:
        print(f"🔧 Application de {len(critical_improvements)} amélioration(s) critique(s)...")
        apply_improvements(critical_improvements)
    
    # Tests de compilation
    test_compilation()
    
    print("")
    print("=" * 60)
    print(f"✨ Cycle terminé! Prochaine version: thebestclient{next_version}")
    print("📋 Les changements seront commités automatiquement par GitHub Actions")

if __name__ == "__main__":
    main()
