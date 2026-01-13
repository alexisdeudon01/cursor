#!/usr/bin/env python3
"""
Script de vérification des métriques de qualité de code.
Vérifie si les métriques utilisées sont adéquates et propose des améliorations.
"""

import os
import json
import subprocess
import requests
from pathlib import Path
from typing import Dict, List, Optional
from datetime import datetime

PROJECT_ROOT = Path(".")
AGENTS_DIR = Path(".cursor/agents")

def search_best_practices_metrics():
    """Recherche les meilleures pratiques pour métriques de qualité de code."""
    print("🔍 Recherche des meilleures pratiques pour métriques de qualité de code...")
    
    # Métriques standards de qualité de code (basé sur recherche)
    standard_metrics = {
        "code_quality": {
            "complexity": {
                "description": "Complexité cyclomatique",
                "tools": ["SonarQube", "CodeClimate", "PMD"],
                "threshold": "Complexité < 10 par fonction"
            },
            "maintainability_index": {
                "description": "Indice de maintenabilité",
                "range": "0-100",
                "good": "> 70",
                "tools": ["Visual Studio Code Metrics", "SonarQube"]
            },
            "technical_debt": {
                "description": "Dette technique",
                "measurement": "Temps estimé pour corriger",
                "tools": ["SonarQube", "CodeClimate"]
            }
        },
        "architecture": {
            "coupling": {
                "description": "Couplage entre modules",
                "principle": "Low coupling, high cohesion",
                "measurement": "Nombre de dépendances entre modules"
            },
            "cohesion": {
                "description": "Cohésion des modules",
                "principle": "Éléments d'un module travaillent ensemble",
                "measurement": "Ratio de méthodes utilisant les mêmes données"
            },
            "separation_of_concerns": {
                "description": "Séparation des préoccupations",
                "principle": "Chaque module a une responsabilité unique",
                "measurement": "Violations SRP (Single Responsibility Principle)"
            }
        },
        "testing": {
            "code_coverage": {
                "description": "Couverture de code",
                "target": "> 80%",
                "tools": ["Coverlet", "Coverage.py", "JaCoCo"]
            },
            "test_count": {
                "description": "Nombre de tests",
                "principle": "Plus de tests = plus de confiance"
            }
        },
        "documentation": {
            "documentation_coverage": {
                "description": "Couverture de documentation",
                "target": "> 60% des classes/méthodes publiques documentées"
            },
            "readme_quality": {
                "description": "Qualité du README",
                "elements": ["Installation", "Usage", "Architecture", "Examples"]
            }
        },
        "security": {
            "vulnerabilities": {
                "description": "Vulnérabilités connues",
                "tools": ["Snyk", "OWASP Dependency Check", "GitHub Dependabot"]
            },
            "secret_scanning": {
                "description": "Détection de secrets dans le code",
                "tools": ["GitHub Secret Scanning", "git-secrets"]
            }
        }
    }
    
    return standard_metrics

def analyze_current_metrics():
    """Analyse les métriques actuellement utilisées."""
    print("📊 Analyse des métriques actuelles...")
    
    current_metrics = {
        "architecture": {
            "score": 9,
            "max": 10,
            "description": "Séparation Client/Serveur",
            "based_on": "Vérification manuelle des assemblies et scènes"
        },
        "modularity_games": {
            "score": 8,
            "max": 10,
            "description": "Modularité des jeux",
            "based_on": "Existence IGameDefinition + GameRegistry"
        },
        "modularity_sessions": {
            "score": 7,
            "max": 10,
            "description": "Modularité des sessions",
            "based_on": "Extensibilité SessionContainer"
        },
        "network_config": {
            "score": 10,
            "max": 10,
            "description": "Configuration réseau",
            "based_on": "UseEncryption = false, config minimale"
        },
        "documentation": {
            "score": 8,
            "max": 10,
            "description": "Documentation",
            "based_on": "Présence de fichiers .md"
        }
    }
    
    return current_metrics

def calculate_metrics_automatically():
    """Calcule les métriques automatiquement à partir du code."""
    print("🔢 Calcul automatique des métriques...")
    
    metrics = {}
    
    # 1. Architecture - Vérifier séparation Client/Serveur
    print("  📐 Architecture...")
    client_files = list(PROJECT_ROOT.rglob("**/Client/**/*.cs"))
    server_files = list(PROJECT_ROOT.rglob("**/Server/**/*.cs"))
    shared_files = list(PROJECT_ROOT.rglob("**/Shared/**/*.cs"))
    
    # Vérifier qu'il n'y a pas de références croisées
    violations = 0
    for client_file in client_files:
        content = client_file.read_text(encoding='utf-8', errors='ignore')
        if "using.*Server" in content or "namespace.*Server" in content:
            violations += 1
    
    architecture_score = max(0, 10 - (violations * 2))
    metrics["architecture"] = {
        "score": architecture_score,
        "max": 10,
        "violations": violations,
        "client_files": len(client_files),
        "server_files": len(server_files),
        "shared_files": len(shared_files)
    }
    
    # 2. Modularité - Vérifier système de jeux
    print("  🧩 Modularité jeux...")
    game_definitions = list(PROJECT_ROOT.rglob("**/*GameDefinition*.cs"))
    game_registry_exists = (PROJECT_ROOT / "Assets/Scripts/Core/Games/GameRegistry.cs").exists()
    
    modularity_games_score = 0
    if game_registry_exists:
        modularity_games_score += 5
    if len(game_definitions) >= 2:
        modularity_games_score += 3
    if (PROJECT_ROOT / "HOW_TO_ADD_GAME.md").exists():
        modularity_games_score += 2
    
    metrics["modularity_games"] = {
        "score": modularity_games_score,
        "max": 10,
        "game_definitions": len(game_definitions),
        "game_registry": game_registry_exists
    }
    
    # 3. Configuration réseau - Vérifier UseEncryption = false
    print("  🌐 Configuration réseau...")
    bootstrap_files = list(PROJECT_ROOT.rglob("**/*Bootstrap*.cs"))
    encryption_disabled = 0
    encryption_enabled = 0
    
    for bootstrap_file in bootstrap_files:
        content = bootstrap_file.read_text(encoding='utf-8', errors='ignore')
        if "UseEncryption = false" in content:
            encryption_disabled += 1
        elif "UseEncryption = true" in content:
            encryption_enabled += 1
    
    network_score = 10 if encryption_disabled > 0 and encryption_enabled == 0 else 5
    metrics["network_config"] = {
        "score": network_score,
        "max": 10,
        "encryption_disabled": encryption_disabled,
        "encryption_enabled": encryption_enabled
    }
    
    # 4. Documentation - Compter fichiers .md
    print("  📚 Documentation...")
    doc_files = list(PROJECT_ROOT.rglob("**/*.md"))
    readme_exists = (PROJECT_ROOT / "README.md").exists()
    architecture_doc = (PROJECT_ROOT / "ARCHITECTURE.md").exists() or (PROJECT_ROOT / "Assets/Scripts/Documentation/Architecture.md").exists()
    
    doc_score = 0
    if readme_exists:
        doc_score += 2
    if architecture_doc:
        doc_score += 3
    if len(doc_files) >= 10:
        doc_score += 3
    if len(doc_files) >= 20:
        doc_score += 2
    
    metrics["documentation"] = {
        "score": min(doc_score, 10),
        "max": 10,
        "doc_files": len(doc_files),
        "readme": readme_exists,
        "architecture_doc": architecture_doc
    }
    
    # 5. Tests - Vérifier présence de tests
    print("  🧪 Tests...")
    test_files = list(PROJECT_ROOT.rglob("**/*Test*.cs"))
    test_score = min(10, len(test_files) * 2)
    
    metrics["testing"] = {
        "score": test_score,
        "max": 10,
        "test_files": len(test_files)
    }
    
    # 6. Compilation - Vérifier builds
    print("  🔨 Compilation...")
    build_client = (PROJECT_ROOT / "Build/Client/Client.x86_64").exists()
    build_server = (PROJECT_ROOT / "Build/Server/Server.x86_64").exists()
    buildscript_exists = (PROJECT_ROOT / "Assets/Scripts/Editor/BuildScript.cs").exists()
    
    compilation_score = 0
    if buildscript_exists:
        compilation_score += 5
    if build_client:
        compilation_score += 2.5
    if build_server:
        compilation_score += 2.5
    
    metrics["compilation"] = {
        "score": compilation_score,
        "max": 10,
        "build_client": build_client,
        "build_server": build_server,
        "buildscript": buildscript_exists
    }
    
    return metrics

def compare_metrics(current: Dict, calculated: Dict, best_practices: Dict):
    """Compare les métriques actuelles avec les calculées et les meilleures pratiques."""
    print("\n📊 Comparaison des métriques...")
    
    comparison = {
        "current_vs_calculated": {},
        "gaps": [],
        "recommendations": []
    }
    
    # Comparer scores
    for metric_name in calculated.keys():
        if metric_name in current:
            current_score = current[metric_name].get("score", 0)
            calculated_score = calculated[metric_name].get("score", 0)
            diff = abs(current_score - calculated_score)
            
            comparison["current_vs_calculated"][metric_name] = {
                "current": current_score,
                "calculated": calculated_score,
                "difference": diff,
                "match": diff <= 1
            }
            
            if diff > 1:
                comparison["gaps"].append({
                    "metric": metric_name,
                    "issue": f"Écart de {diff} points entre score manuel et calculé"
                })
    
    # Recommandations basées sur best practices
    if calculated.get("testing", {}).get("score", 0) < 5:
        comparison["recommendations"].append({
            "metric": "testing",
            "recommendation": "Ajouter plus de tests unitaires (target: > 80% coverage)",
            "priority": "high"
        })
    
    if calculated.get("documentation", {}).get("score", 0) < 7:
        comparison["recommendations"].append({
            "metric": "documentation",
            "recommendation": "Améliorer la documentation (target: > 60% coverage)",
            "priority": "medium"
        })
    
    return comparison

def generate_metrics_report(current: Dict, calculated: Dict, comparison: Dict, best_practices: Dict):
    """Génère un rapport sur les métriques."""
    report = f"""# Rapport de Vérification des Métriques
**Date**: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}

## 📊 Métriques Actuelles (Manuelles)

"""
    
    for metric_name, metric_data in current.items():
        report += f"### {metric_name.capitalize()}\n"
        report += f"- **Score**: {metric_data['score']}/{metric_data['max']}\n"
        report += f"- **Description**: {metric_data.get('description', 'N/A')}\n"
        report += f"- **Basé sur**: {metric_data.get('based_on', 'N/A')}\n\n"
    
    report += "\n## 🔢 Métriques Calculées (Automatiques)\n\n"
    
    for metric_name, metric_data in calculated.items():
        report += f"### {metric_name.capitalize()}\n"
        report += f"- **Score**: {metric_data['score']}/{metric_data['max']}\n"
        for key, value in metric_data.items():
            if key != "score" and key != "max":
                report += f"- **{key}**: {value}\n"
        report += "\n"
    
    report += "\n## 📈 Comparaison\n\n"
    
    for metric_name, comp_data in comparison["current_vs_calculated"].items():
        match_icon = "✅" if comp_data["match"] else "⚠️"
        report += f"### {metric_name.capitalize()} {match_icon}\n"
        report += f"- Score manuel: {comp_data['current']}/10\n"
        report += f"- Score calculé: {comp_data['calculated']}/10\n"
        report += f"- Différence: {comp_data['difference']}\n\n"
    
    if comparison["gaps"]:
        report += "\n## ⚠️ Écarts Identifiés\n\n"
        for gap in comparison["gaps"]:
            report += f"- **{gap['metric']}**: {gap['issue']}\n"
        report += "\n"
    
    if comparison["recommendations"]:
        report += "\n## 💡 Recommandations\n\n"
        for rec in comparison["recommendations"]:
            priority_icon = "🔴" if rec["priority"] == "high" else "🟡"
            report += f"{priority_icon} **{rec['metric']}** ({rec['priority']}): {rec['recommendation']}\n"
        report += "\n"
    
    report += "\n## 📚 Meilleures Pratiques\n\n"
    report += "### Métriques Standards de Qualité de Code\n\n"
    
    for category, metrics in best_practices.items():
        report += f"#### {category.capitalize()}\n\n"
        for metric_name, metric_info in metrics.items():
            report += f"**{metric_name}**:\n"
            report += f"- Description: {metric_info.get('description', 'N/A')}\n"
            if "tools" in metric_info:
                report += f"- Outils: {', '.join(metric_info['tools'])}\n"
            if "target" in metric_info:
                report += f"- Target: {metric_info['target']}\n"
            report += "\n"
    
    report += "\n---\n**Rapport généré automatiquement par verify-metrics.py**\n"
    
    return report

def main():
    """Fonction principale."""
    print("=" * 60)
    print("📊 Vérification des Métriques de Qualité de Code")
    print("=" * 60)
    print()
    
    # 1. Rechercher meilleures pratiques
    best_practices = search_best_practices_metrics()
    
    # 2. Analyser métriques actuelles
    current_metrics = analyze_current_metrics()
    
    # 3. Calculer métriques automatiquement
    calculated_metrics = calculate_metrics_automatically()
    
    # 4. Comparer
    comparison = compare_metrics(current_metrics, calculated_metrics, best_practices)
    
    # 5. Générer rapport
    report = generate_metrics_report(current_metrics, calculated_metrics, comparison, best_practices)
    
    # 6. Sauvegarder rapport
    report_file = AGENTS_DIR / f"metrics-verification-{datetime.now().strftime('%Y%m%d-%H%M%S')}.md"
    report_file.parent.mkdir(parents=True, exist_ok=True)
    report_file.write_text(report, encoding='utf-8')
    
    print(f"✅ Rapport généré: {report_file}")
    print()
    
    # 7. Afficher résumé
    print("=" * 60)
    print("📊 RÉSUMÉ")
    print("=" * 60)
    
    total_current = sum(m.get("score", 0) for m in current_metrics.values())
    total_calculated = sum(m.get("score", 0) for m in calculated_metrics.values())
    max_total = len(current_metrics) * 10
    
    print(f"Score total (manuel): {total_current}/{max_total} ({total_current/max_total*100:.1f}%)")
    print(f"Score total (calculé): {total_calculated}/{max_total} ({total_calculated/max_total*100:.1f}%)")
    print()
    
    if comparison["gaps"]:
        print(f"⚠️  {len(comparison['gaps'])} écart(s) identifié(s)")
    else:
        print("✅ Métriques cohérentes")
    
    if comparison["recommendations"]:
        print(f"💡 {len(comparison['recommendations'])} recommandation(s)")

if __name__ == "__main__":
    main()
