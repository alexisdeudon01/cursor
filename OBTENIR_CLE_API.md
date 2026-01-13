# 🔐 Comment Obtenir et Configurer la Clé API Anthropic

## ⚠️ Important

**Je ne peux pas te donner la clé directement** car :
- Les clés API sont des secrets sensibles
- Elles doivent être récupérées depuis ton compte Anthropic
- Elles ne doivent jamais être partagées publiquement

## 📋 Étapes pour obtenir la clé

### Étape 1: Aller sur le site Anthropic

1. Ouvrir https://console.anthropic.com/
2. Se connecter avec ton compte Anthropic

### Étape 2: Créer ou récupérer une clé API

1. Aller dans **Settings** ou **API Keys**
2. Cliquer sur **Create Key** ou **New API Key**
3. Donner un nom (ex: "GitHub Actions - Cursor Project")
4. **Copier la clé** immédiatement (elle ne sera affichée qu'une fois)

### Étape 3: Format de la clé

La clé doit ressembler à :
```
sk-ant-api03-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
```

**Important** :
- ✅ Commence par `sk-ant-api03-` ou `sk-ant-`
- ✅ Très longue (plusieurs dizaines de caractères)
- ✅ Pas d'espaces
- ✅ Pas de sauts de ligne

### Étape 4: Ajouter dans GitHub Secrets

1. Aller sur GitHub : https://github.com/alexisdeudon01/cursor
2. **Settings** → **Secrets and variables** → **Actions**
3. Cliquer sur **New repository secret**
4. **Name**: `ANTHROPIC_API_KEY`
5. **Secret**: Coller la clé API (sans espaces, sans sauts de ligne)
6. Cliquer sur **Add secret**

### Étape 5: Vérifier

Le prochain cycle GitHub Actions devrait afficher :
```
✅ API Anthropic accessible
```

au lieu de :
```
❌ Erreur API: 401
```

## 🔍 Si tu as déjà une clé

Si tu as déjà une clé API :
1. Aller sur https://console.anthropic.com/
2. **Settings** → **API Keys**
3. Voir tes clés existantes
4. Si nécessaire, créer une nouvelle clé
5. Copier et ajouter dans GitHub Secrets

## ⚠️ Sécurité

- ❌ **Ne jamais** partager ta clé API publiquement
- ❌ **Ne jamais** la mettre dans le code
- ❌ **Ne jamais** la commiter dans Git
- ✅ **Toujours** utiliser GitHub Secrets
- ✅ **Toujours** vérifier qu'elle n'est pas dans l'historique Git

## 🧪 Test après configuration

Une fois la clé ajoutée dans GitHub Secrets, le prochain cycle GitHub Actions devrait :
1. ✅ Détecter la clé
2. ✅ Se connecter à l'API Anthropic
3. ✅ Utiliser l'IA Claude pour l'analyse
4. ✅ Générer des jeux 2D avec le LLM

---

**Note**: Si tu as des problèmes pour obtenir la clé, vérifie que tu as un compte Anthropic valide avec des crédits disponibles.
