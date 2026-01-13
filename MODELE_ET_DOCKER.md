# 🤖 Modèle Utilisé et Vérification Docker

## 🤖 Modèle AI utilisé

### Modèle actuel

**Modèle**: `claude-3-5-sonnet-20241022`

**Utilisé dans** :
- ✅ `.github/scripts/auto-improve-ai.py` - Analyse de code
- ✅ `.github/scripts/train-llm-games.py` - Entraînement LLM jeux 2D
- ✅ `.github/scripts/check-api-access.py` - Tests de connexion

**Version API**: `2024-06-20` (mise à jour depuis 2023-06-01)

### Caractéristiques du modèle

- **Nom complet**: Claude 3.5 Sonnet (20241022)
- **Type**: LLM multimodal (texte)
- **Provider**: Anthropic
- **Coût**: 
  - Input: $0.003/1K tokens
  - Output: $0.015/1K tokens

### Alternatives disponibles

Si besoin de changer de modèle :
- `claude-3-5-sonnet-20240620` - Version alternative
- `claude-3-opus-20240229` - Plus puissant mais plus cher
- `claude-3-haiku-20240307` - Plus rapide et moins cher

## 🐳 Vérification Docker

### Configuration Docker

**Image de base**: `unityci/editor:6000.3.0f1-linux-il2cpp-1.0.0`

**Fichiers Docker** :
- ✅ `Dockerfile` - Image Unity 6000.3.0f1
- ✅ `docker-compose.yml` - Configuration Docker Compose
- ✅ `.dockerignore` - Fichiers exclus
- ✅ `Assets/Scripts/Editor/BuildScript.cs` - Script de build Unity
- ✅ `.github/scripts/build-unity.sh` - Script shell pour builds

### Test Docker

Pour tester Docker localement :

```bash
# Build de l'image
docker build -t unity-6000.3.0f1-builder -f Dockerfile .

# Test build Client
docker run --rm -v $PWD:/workspace unity-6000.3.0f1-builder /usr/local/bin/build-unity.sh client

# Test build Serveur
docker run --rm -v $PWD:/workspace unity-6000.3.0f1-builder /usr/local/bin/build-unity.sh server
```

### Utilisation dans GitHub Actions

Le workflow GitHub Actions :
1. Build l'image Docker Unity
2. Lance les builds Client et Serveur
3. Continue même si échec (normal sans licence Unity)

## 📊 Résumé

### Modèle AI
- **Modèle**: Claude 3.5 Sonnet 20241022 ✅
- **API Version**: 2024-06-20 ✅
- **Coût**: ~$5.76/mois (toutes les 2 heures)

### Docker
- **Image**: Unity 6000.3.0f1 ✅
- **BuildScript.cs**: Créé ✅
- **build-unity.sh**: Créé ✅
- **Workflow**: Intégré ✅

---

**Vérifié le**: 2026-01-13
