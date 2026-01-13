#!/usr/bin/env python3
"""
Script de test pour vérifier si le modèle et l'API Anthropic fonctionnent correctement.
"""

import os
import requests
import json

ANTHROPIC_API_KEY = os.getenv("ANTHROPIC_API_KEY")

def test_model_access():
    """Teste l'accès au modèle avec différentes versions d'API."""
    
    if not ANTHROPIC_API_KEY:
        print("❌ ANTHROPIC_API_KEY non configuré")
        print("💡 Configurez la clé dans GitHub Secrets ou .github/KEYS.txt")
        return
    
    url = "https://api.anthropic.com/v1/messages"
    
    # Test avec version API actuelle (2024-06-20)
    api_versions = [
        "2024-06-20",  # Version actuelle recommandée
        "2023-06-01",  # Version actuellement utilisée
    ]
    
    models = [
        "claude-3-5-sonnet-20241022",  # Modèle actuellement utilisé
        "claude-3-5-sonnet-20240620",  # Version alternative
        "claude-3-opus-20240229",      # Alternative
    ]
    
    print("🧪 Test de connexion API Anthropic")
    print("=" * 60)
    
    for api_version in api_versions:
        print(f"\n📡 Test avec API version: {api_version}")
        for model in models:
            headers = {
                "x-api-key": ANTHROPIC_API_KEY,
                "anthropic-version": api_version,
                "content-type": "application/json"
            }
            
            data = {
                "model": model,
                "max_tokens": 10,
                "messages": [{"role": "user", "content": "Test"}]
            }
            
            try:
                response = requests.post(url, headers=headers, json=data, timeout=10)
                
                if response.status_code == 200:
                    print(f"  ✅ {model}: OK")
                elif response.status_code == 404:
                    print(f"  ❌ {model}: 404 - Modèle non trouvé")
                    print(f"     Réponse: {response.text[:200]}")
                elif response.status_code == 401:
                    print(f"  ❌ {model}: 401 - Clé API invalide")
                elif response.status_code == 400:
                    print(f"  ⚠️  {model}: 400 - Requête invalide")
                    print(f"     Réponse: {response.text[:200]}")
                else:
                    print(f"  ❌ {model}: {response.status_code}")
                    print(f"     Réponse: {response.text[:200]}")
                    
            except Exception as e:
                print(f"  ❌ {model}: Erreur - {e}")
    
    print("\n" + "=" * 60)
    print("💡 Recommandation: Utiliser la version API la plus récente qui fonctionne")

if __name__ == "__main__":
    test_model_access()
