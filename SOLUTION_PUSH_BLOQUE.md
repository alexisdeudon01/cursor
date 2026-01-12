# 🔧 Solution Push Bloqué par GitHub

## Problème
```
remote: error: GH013: Repository rule violations found for refs/heads/dev.
remote: - GITHUB PUSH PROTECTION
remote: - Push cannot contain secrets
remote: - Anthropic API Key
remote: - commit: 7e0526684337d1e2ab3a60bba11e46033c4cc2c0
remote: - path: .github/KEYS.txt:1
```

## ✅ Solution: Retirer KEYS.txt de Git

### Commandes à exécuter

```bash
cd /home/tor/wkspaces/mo2

# 1. Retirer KEYS.txt de Git (mais garder le fichier local)
git rm --cached .github/KEYS.txt

# 2. S'assurer qu'il est dans .gitignore
echo ".github/KEYS.txt" >> .gitignore

# 3. Commit
git add .gitignore
git commit -m "🔐 Retrait KEYS.txt de Git"

# 4. Push
git push origin dev
```

## Si toujours bloqué

Le commit `7e05266` est dans l'historique. Options:

### Option 1: Autoriser via GitHub (temporaire)

Clique sur ce lien:
https://github.com/alexisdeudon01/cursor/security/secret-scanning/unblock-secret/38AwVNhQV4g1IBfG20S4RH2zeHb

⚠️ **Ce n'est pas recommandé** - mieux vaut nettoyer l'historique.

### Option 2: Nettoyer l'historique (recommandé)

```bash
# Créer une branche propre depuis un commit avant 7e05266
git log --oneline | grep -B5 "7e05266"

# Trouver le commit avant 7e05266, puis:
git checkout -b dev-clean <commit-avant-7e05266>

# Appliquer tous les commits sauf celui avec KEYS.txt
# (cherry-pick les commits propres)

# Remplacer dev
git push origin dev-clean:dev --force
```

### Option 3: Utiliser le script

```bash
cd /home/tor/wkspaces/mo2
chmod +x fix-push-blocked.sh
./fix-push-blocked.sh
```

## Vérification

```bash
# Vérifier que KEYS.txt n'est plus dans Git
git ls-files | grep KEYS.txt
# Ne doit rien afficher

# Vérifier que KEYS.txt existe localement
ls -la .github/KEYS.txt
# Doit afficher le fichier
```

## Commandes complètes (copier-coller)

```bash
cd /home/tor/wkspaces/mo2 && \
git rm --cached .github/KEYS.txt && \
echo ".github/KEYS.txt" >> .gitignore && \
git add .gitignore && \
git commit -m "🔐 Retrait KEYS.txt de Git" && \
git push origin dev
```

Si ça échoue encore, utilise le lien GitHub pour autoriser temporairement, puis nettoie l'historique.
