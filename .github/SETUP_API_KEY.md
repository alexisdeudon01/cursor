# Configuration de la clé API Anthropic

## ⚠️ IMPORTANT: Ne jamais commiter la clé API dans le code !

## Méthode 1: Via l'interface GitHub (Recommandé)

1. Aller sur ton repo GitHub: https://github.com/alexisdeudon01/cursor
2. Cliquer sur **Settings** (en haut à droite)
3. Dans le menu de gauche, cliquer sur **Secrets and variables** → **Actions**
4. Cliquer sur **New repository secret**
5. **Name**: `ANTHROPIC_API_KEY`
6. **Secret**: Coller ta clé API
7. Cliquer sur **Add secret**

## Méthode 2: Via GitHub CLI (si installé)

```bash
gh secret set ANTHROPIC_API_KEY --repo alexisdeudon01/cursor
# Puis coller la clé quand demandé
```

## Méthode 3: Via l'API GitHub (script)

Exécuter ce script (remplacer YOUR_GITHUB_TOKEN par un token avec permissions repo):

```bash
curl -X POST \
  -H "Authorization: token YOUR_GITHUB_TOKEN" \
  -H "Accept: application/vnd.github.v3+json" \
  https://api.github.com/repos/alexisdeudon01/cursor/actions/secrets/ANTHROPIC_API_KEY \
  -d '{"encrypted_value":"VOTRE_CLE_ENCRYPTEE","key_id":"KEY_ID"}'
```

⚠️ Cette méthode nécessite d'encrypter la clé avec la clé publique du repo (complexe).

## Vérification

Une fois la clé ajoutée:
1. Aller dans **Actions** sur GitHub
2. Lancer manuellement le workflow "Auto-Improve Project (Thebestclient)"
3. Vérifier dans les logs qu'on voit: "🤖 Utilisation de l'IA Claude pour amélioration..."

## Ta clé API

⚠️ **NE JAMAIS COMMITER CETTE CLÉ DANS LE CODE !**

Ta clé API (à ajouter dans GitHub Secrets):
```
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
```

## Sécurité

- ✅ La clé sera stockée de manière sécurisée dans GitHub Secrets
- ✅ Elle ne sera jamais visible dans les logs GitHub Actions
- ✅ Seul le workflow pourra y accéder
- ❌ Ne jamais la mettre dans un fichier .env versionné
- ❌ Ne jamais la commiter dans le code
