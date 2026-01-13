#!/usr/bin/env python3
"""
Script de vérification d'accès à l'API Anthropic et Git.
"""

import os
import sys
import requests
import subprocess

ANTHROPIC_API_KEY = os.getenv("ANTHROPIC_API_KEY")
GITHUB_TOKEN = os.getenv("GITHUB_TOKEN")
GITHUB_REPOSITORY = os.getenv("GITHUB_REPOSITORY")

def test_anthropic_api():
    """Teste l'accès à l'API Anthropic."""
    print("🔍 Test API Anthropic...")
    
    if not ANTHROPIC_API_KEY:
        print("  ❌ ANTHROPIC_API_KEY non configuré")
        return False
    
    url = "https://api.anthropic.com/v1/messages"
    headers = {
        "x-api-key": ANTHROPIC_API_KEY,
        "anthropic-version": "2024-06-20",  # Version API mise à jour (était 2023-06-01)
        "content-type": "application/json"
    }
    
    data = {
        "model": "claude-3-5-sonnet-20241022",
        "max_tokens": 10,
        "messages": [{"role": "user", "content": "Test"}]
    }
    
    try:
        response = requests.post(url, headers=headers, json=data, timeout=10)
        if response.status_code == 200:
            print("  ✅ API Anthropic accessible")
            return True
        else:
            print(f"  ❌ Erreur API: {response.status_code}")
            print(f"     {response.text[:200]}")
            return False
    except Exception as e:
        print(f"  ❌ Erreur connexion: {e}")
        return False

def test_git_access():
    """Teste l'accès Git."""
    print("🔍 Test Git...")
    
    try:
        result = subprocess.run(
            ["git", "remote", "-v"],
            capture_output=True,
            text=True,
            timeout=5
        )
        if result.returncode == 0:
            print("  ✅ Git configuré")
            print(f"     Remote: {result.stdout.strip().split()[1] if result.stdout else 'N/A'}")
            return True
        else:
            print("  ❌ Git non configuré")
            return False
    except Exception as e:
        print(f"  ❌ Erreur Git: {e}")
        return False

def test_github_token():
    """Teste le token GitHub."""
    print("🔍 Test GitHub Token...")
    
    if not GITHUB_TOKEN:
        print("  ⚠️  GITHUB_TOKEN non configuré (normal en local)")
        return None
    
    try:
        headers = {"Authorization": f"token {GITHUB_TOKEN}"}
        response = requests.get("https://api.github.com/user", headers=headers, timeout=10)
        if response.status_code == 200:
            print("  ✅ GitHub Token valide")
            return True
        else:
            print(f"  ❌ Token invalide: {response.status_code}")
            return False
    except Exception as e:
        print(f"  ❌ Erreur: {e}")
        return False

def main():
    """Fonction principale."""
    print("=" * 60)
    print("🧪 Tests de connexion")
    print("=" * 60)
    print()
    
    results = {
        "anthropic": test_anthropic_api(),
        "git": test_git_access(),
        "github": test_github_token()
    }
    
    print()
    print("=" * 60)
    print("📊 Résumé")
    print("=" * 60)
    
    for name, result in results.items():
        status = "✅" if result else ("⚠️" if result is None else "❌")
        print(f"  {status} {name}")
    
    all_ok = all(r for r in results.values() if r is not None)
    
    if all_ok:
        print()
        print("✅ Tous les tests sont passés!")
        sys.exit(0)
    else:
        print()
        print("⚠️  Certains tests ont échoué")
        sys.exit(1)

if __name__ == "__main__":
    main()
