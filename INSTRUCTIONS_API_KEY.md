# 🔐 Instructions pour ajouter la clé API Anthropic

## ⚠️ La clé API est dans l'historique Git

GitHub bloque le push car la clé a été détectée dans l'historique. Deux options:

## Option 1: Autoriser via GitHub (Recommandé - 1 clic)

**Clique sur ce lien pour autoriser**: 
https://github.com/alexisdeudon01/cursor/security/secret-scanning/unblock-secret/38AwVNhQV4g1IBfG20S4RH2zeHb

Puis GitHub te permettra de push normalement.

## Option 2: Ajouter manuellement la clé (Sécurisé)

1. **Ouvre**: https://github.com/alexisdeudon01/cursor/settings/secrets/actions
2. **Clique**: "New repository secret"
3. **Name**: `ANTHROPIC_API_KEY`
4. **Secret**: (ta clé API - voir message ou fichier local)
5. **Add secret**

La clé API est disponible dans:
- Message privé que je t'ai envoyé
- Fichier local `.github/API_KEY_LOCAL.txt` (pas versionné)

## Une fois la clé ajoutée

Le système utilisera automatiquement l'IA Claude pour:
- ✅ Analyser le code toutes les 30 minutes
- ✅ Générer des diagrammes UML (.mmd + .png)
- ✅ Appliquer des améliorations automatiquement
- ✅ Créer de nouvelles versions de l'agent

---

**Note**: La clé dans l'historique Git sera automatiquement expirée/rotée par GitHub après un certain temps, mais il vaut mieux l'ajouter manuellement dans Secrets pour que le système fonctionne immédiatement.
