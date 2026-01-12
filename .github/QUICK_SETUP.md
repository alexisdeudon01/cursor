# 🚀 Configuration Rapide - Clé API Anthropic

## Option 1: Interface Web GitHub (Le plus simple - 2 minutes)

1. **Ouvre ce lien**: https://github.com/alexisdeudon01/cursor/settings/secrets/actions

2. **Clique sur**: "New repository secret" (bouton en haut à droite)

3. **Remplis**:
   - **Name**: `ANTHROPIC_API_KEY`
   - **Secret**: `[TA_CLÉ_API_ICI]` (voir message privé ou fichier local)

4. **Clique sur**: "Add secret"

✅ **C'est tout !** Le système utilisera automatiquement l'IA dans 30 minutes.

---

## Option 2: GitHub CLI (Si tu as `gh` installé)

```bash
cd /workspace
./.github/scripts/setup-api-key.sh
```

---

## Vérification

Pour vérifier que ça marche:

1. Va dans **Actions** sur GitHub: https://github.com/alexisdeudon01/cursor/actions
2. Clique sur **"Auto-Improve Project (Thebestclient)"**
3. Clique sur **"Run workflow"** → **"Run workflow"**
4. Dans les logs, tu devrais voir: **"🤖 Utilisation de l'IA Claude pour amélioration..."**

---

## ⚠️ Important

- ✅ La clé est stockée de manière sécurisée dans GitHub Secrets
- ✅ Elle ne sera jamais visible dans les logs
- ❌ Ne jamais la commiter dans le code
- ❌ Ne jamais la partager publiquement

---

**Une fois configuré, le système fonctionnera automatiquement toutes les 30 minutes !** 🎉
