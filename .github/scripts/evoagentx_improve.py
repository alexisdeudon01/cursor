#!/usr/bin/env python3
"""
Phase 2: EvoAgentX - Amélioration basée sur l'AGENT
L'agent.md est la SOURCE DE VÉRITÉ pour toutes les améliorations
"""
import os
import json
import re
import requests
from pathlib import Path
from datetime import datetime

API_KEY = os.environ.get("ANTHROPIC_API_KEY")
API_URL = "https://api.anthropic.com/v1/messages"

def call_claude(prompt, max_tokens=4096):
    """Appelle Claude API avec timeout étendu"""
    if not API_KEY:
        print("❌ ANTHROPIC_API_KEY non définie")
        return None
    
    try:
        print("📡 Appel Claude API (timeout: 120s)...")
        response = requests.post(
            API_URL,
            headers={
                "x-api-key": API_KEY,
                "anthropic-version": "2023-06-01",
                "content-type": "application/json"
            },
            json={
                "model": "claude-sonnet-4-20250514",
                "max_tokens": max_tokens,
                "messages": [{"role": "user", "content": prompt}]
            },
            timeout=120
        )
        
        if response.status_code == 401:
            print("❌ Clé API invalide")
            return None
        if response.status_code == 429:
            print("⚠️ Rate limit, attente 30s...")
            import time
            time.sleep(30)
            return call_claude(prompt, max_tokens)
        
        response.raise_for_status()
        return response.json()["content"][0]["text"]
    
    except requests.exceptions.Timeout:
        print("❌ Timeout API (120s)")
        return None
    except Exception as e:
        print(f"❌ Erreur: {e}")
        return None

def load_agent():
    """Charge l'agent - SOURCE DE VÉRITÉ"""
    agent_path = Path(".cursor/agents/agent.md")
    if agent_path.exists():
        content = agent_path.read_text()
        print(f"✅ Agent chargé: {len(content)} caractères")
        return content
    print("⚠️ Agent non trouvé!")
    return ""

def load_metrics():
    """Charge les métriques générées par Claude"""
    metrics_file = Path(".github/reports/current_metrics.json")
    if metrics_file.exists():
        return json.loads(metrics_file.read_text())
    return {"improvements": [], "total_score": 50}

def discover_project():
    """Découvre la structure du projet"""
    project = {
        "scripts": {},
        "uxml": {},
        "uss": {},
        "scenes": [],
        "structure": []
    }
    
    # Scripts C#
    for f in Path("Assets/Scripts").rglob("*.cs"):
        try:
            project["scripts"][str(f)] = f.read_text()
        except:
            pass
    
    # UI Toolkit UXML
    for f in Path("Assets/UI").rglob("*.uxml"):
        try:
            project["uxml"][str(f)] = f.read_text()
        except:
            pass
    
    # UI Toolkit USS
    for f in Path("Assets/UI").rglob("*.uss"):
        try:
            project["uss"][str(f)] = f.read_text()
        except:
            pass
    
    # Scènes
    for f in Path("Assets/Scenes").rglob("*.unity"):
        project["scenes"].append(str(f))
    
    # Structure générale
    for f in Path("Assets").rglob("*"):
        if f.is_file():
            project["structure"].append(str(f))
    
    return project

def generate_improvements(agent, metrics, project):
    """
    Génère des améliorations en utilisant l'AGENT comme instructions
    """
    # Construire le contexte des fichiers
    scripts_content = "\n\n".join([
        f"### {path}\n```csharp\n{content}\n```"
        for path, content in list(project["scripts"].items())[:10]
    ])
    
    uxml_content = "\n\n".join([
        f"### {path}\n```xml\n{content}\n```"
        for path, content in list(project["uxml"].items())[:5]
    ])
    
    uss_content = "\n\n".join([
        f"### {path}\n```css\n{content}\n```"
        for path, content in list(project["uss"].items())[:3]
    ])
    
    prompt = f"""# MISSION: AMÉLIORER LE PROJET SELON L'AGENT

## 🎯 AGENT (TES INSTRUCTIONS - À SUIVRE STRICTEMENT)

{agent}

---

## 📊 MÉTRIQUES ACTUELLES

Score total: {metrics.get('total_score', 'N/A')}

Améliorations prioritaires identifiées:
{json.dumps(metrics.get('improvements', []), indent=2)}

---

## 📁 CODE ACTUEL DU PROJET

### Scripts C# ({len(project['scripts'])} fichiers)
{scripts_content[:8000]}

### UXML ({len(project['uxml'])} fichiers)
{uxml_content[:3000]}

### USS ({len(project['uss'])} fichiers)
{uss_content[:2000]}

### Scènes
{json.dumps(project['scenes'], indent=2)}

---

## TA TÂCHE

En suivant STRICTEMENT les règles de l'agent ci-dessus, analyse le code et propose des améliorations.

Vérifie particulièrement:
1. **Server Authority (25%)**: StartServer() pas StartHost(), logique serveur uniquement
2. **Single Build (15%)**: Un seul exécutable, distinction par scènes
3. **UI Toolkit (20%)**: UXML + USS, PAS de Canvas legacy
4. **Structure Discovery (15%)**: Lecture .unity/.meta via GUIDs
5. **Network Flow (15%)**: Séquence connexion correcte

Réponds avec un JSON valide contenant les fichiers à créer/modifier:

```json
{{
  "analysis": {{
    "server_authority": {{"score": X, "issues": ["..."], "ok": ["..."]}},
    "single_build": {{"score": X, "issues": ["..."], "ok": ["..."]}},
    "ui_toolkit": {{"score": X, "issues": ["..."], "ok": ["..."]}},
    "structure_discovery": {{"score": X, "issues": ["..."], "ok": ["..."]}},
    "network_flow": {{"score": X, "issues": ["..."], "ok": ["..."]}}
  }},
  "files_to_create": [
    {{"path": "Assets/Scripts/...", "content": "...", "reason": "..."}}
  ],
  "files_to_modify": [
    {{"path": "Assets/Scripts/...", "changes": "description des changements", "new_content": "..."}}
  ],
  "agent_improvements": {{
    "should_update": true/false,
    "new_sections": ["suggestions pour améliorer l'agent lui-même"]
  }}
}}
```

IMPORTANT: Le JSON doit être valide et parsable.
"""
    
    return call_claude(prompt, max_tokens=8000)

def apply_improvements(improvements_json):
    """Applique les améliorations au projet"""
    try:
        # Extraire le JSON
        start = improvements_json.find("{")
        end = improvements_json.rfind("}") + 1
        if start < 0 or end <= start:
            print("⚠️ JSON non trouvé dans la réponse")
            return False
        
        improvements = json.loads(improvements_json[start:end])
        
        # Créer les nouveaux fichiers
        files_created = 0
        for file_info in improvements.get("files_to_create", []):
            path = Path(file_info["path"])
            content = file_info.get("content", "")
            reason = file_info.get("reason", "")
            
            if content and len(content) > 50:  # Éviter les fichiers vides
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text(content)
                print(f"✅ Créé: {path} ({reason})")
                files_created += 1
        
        # Modifier les fichiers existants
        files_modified = 0
        for file_info in improvements.get("files_to_modify", []):
            path = Path(file_info["path"])
            new_content = file_info.get("new_content", "")
            
            if path.exists() and new_content and len(new_content) > 50:
                path.write_text(new_content)
                print(f"✅ Modifié: {path}")
                files_modified += 1
        
        # Améliorer l'agent lui-même si suggéré
        agent_updates = improvements.get("agent_improvements", {})
        if agent_updates.get("should_update") and agent_updates.get("new_sections"):
            agent_path = Path(".cursor/agents/agent.md")
            if agent_path.exists():
                agent_content = agent_path.read_text()
                # Ajouter une section "Améliorations suggérées"
                suggestions = "\n".join([f"- {s}" for s in agent_updates["new_sections"]])
                if "## Améliorations suggérées" not in agent_content:
                    agent_content += f"\n\n## Améliorations suggérées (auto-générées)\n\n{suggestions}\n"
                    agent_path.write_text(agent_content)
                    print("✅ Agent mis à jour avec suggestions")
        
        # Sauvegarder le rapport d'analyse
        analysis = improvements.get("analysis", {})
        report_path = Path(".github/reports/improvement_analysis.json")
        report_path.write_text(json.dumps(analysis, indent=2))
        
        print(f"\n📊 Résumé: {files_created} créés, {files_modified} modifiés")
        return True
        
    except json.JSONDecodeError as e:
        print(f"⚠️ Erreur parsing JSON: {e}")
        return False
    except Exception as e:
        print(f"⚠️ Erreur application: {e}")
        return False

def main():
    print("="*60)
    print("🤖 PHASE 2: EVOAGENTX - AMÉLIORATION BASÉE SUR L'AGENT")
    print("="*60)
    
    # 1. Charger l'AGENT (source de vérité)
    print("\n📋 Chargement de l'Agent (SOURCE DE VÉRITÉ)...")
    agent = load_agent()
    if not agent:
        print("❌ Impossible de continuer sans agent")
        return
    
    # 2. Charger les métriques
    print("\n📊 Chargement des métriques...")
    metrics = load_metrics()
    print(f"   Score actuel: {metrics.get('total_score', 'N/A')}")
    
    # 3. Découvrir le projet
    print("\n🔍 Découverte du projet...")
    project = discover_project()
    print(f"   Scripts: {len(project['scripts'])}")
    print(f"   UXML: {len(project['uxml'])}")
    print(f"   USS: {len(project['uss'])}")
    print(f"   Scènes: {len(project['scenes'])}")
    
    # 4. Générer les améliorations (Claude utilise l'agent comme instructions)
    print("\n🧠 Génération des améliorations selon l'Agent...")
    improvements = generate_improvements(agent, metrics, project)
    
    if improvements:
        print("\n📝 Application des améliorations...")
        success = apply_improvements(improvements)
        
        if success:
            print("\n✅ Améliorations appliquées avec succès")
        else:
            print("\n⚠️ Certaines améliorations n'ont pas pu être appliquées")
        
        # Sauvegarder la réponse brute pour debug
        Path(".github/reports/raw_improvements.md").write_text(improvements)
    else:
        print("\n⚠️ Pas de réponse de Claude - mode dégradé")
    
    print("\n" + "="*60)
    print("✅ PHASE 2 TERMINÉE")
    print("="*60)

if __name__ == "__main__":
    main()
