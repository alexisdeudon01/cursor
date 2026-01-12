# 🔧 Fix Push Bloqué par GitHub

## Problème
GitHub bloque le push car KEYS.txt contient la clé API dans un commit.

## ✅ Solution: Retirer KEYS.txt de Git

### Commandes à exécuter

```bash
cd /home/tor/wkspaces/mo2

# 1. Retirer KEYS.txt de Git (mais garder le fichier local)
git rm --cached .github/KEYS.txt

# 2. S'assurer qu'il est dans .gitignore
echo ".github/KEYS.txt" >> .gitignore

# 3. Commit le changement
git add .gitignore
git commit -m "🔐 Retrait KEYS.txt de Git (dans .gitignore)"

# 4. Push
git push origin dev
```

## Alternative: Nettoyer l'historique

Si le commit 7e05266 contient la clé, tu peux:

### Option 1: Réécrire le commit

```bash
# Retirer KEYS.txt du dernier commit
git reset --soft HEAD~1
git rm --cached .github/KEYS.txt
echo ".github/KEYS.txt" >> .gitignore
git add .gitignore
git commit -m "🔐 Setup sans KEYS.txt dans Git"
git push origin dev
```

### Option 2: Autoriser via GitHub (temporaire)

Clique sur le lien fourni par GitHub:
https://github.com/alexisdeudon01/cursor/security/secret-scanning/unblock-secret/38AwVNhQV4g1IBfG20S4RH2zeHb

⚠️ **Mais mieux vaut retirer la clé de Git!**

## Vérification

```bash
# Vérifier que KEYS.txt n'est plus dans Git
git ls-files | grep KEYS.txt
# Ne doit rien afficher

# Vérifier que KEYS.txt existe localement
ls -la .github/KEYS.txt
# Doit afficher le fichier

# Vérifier .gitignore
grep "KEYS.txt" .gitignore
# Doit afficher: .github/KEYS.txt
```

## Commandes complètes (copier-coller)

```bash
cd /home/tor/wkspaces/mo2 && \
git rm --cached .github/KEYS.txt && \
echo ".github/KEYS.txt" >> .gitignore && \
git add .gitignore && \
git commit -m "🔐 Retrait KEYS.txt de Git" && \
echo "✅ Terminé! Maintenant: git push origin dev"
```
