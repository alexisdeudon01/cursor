# 🚀 Démarrage rapide GitHub

## ✅ Ce qui est déjà fait
- ✅ Dépôt Git initialisé
- ✅ Branche `branché-1` créée et active
- ✅ `.gitignore` configuré pour Unity
- ✅ Tous les fichiers ajoutés et commités (719 fichiers)

## 📋 Pour finaliser la connexion à GitHub

### Option 1 : Via l'interface web GitHub (RECOMMANDÉ)

1. **Créer le dépôt sur GitHub** :
   - Allez sur https://github.com/new
   - Nom du dépôt : `cursor`
   - Choisissez Public ou Private
   - ⚠️ **NE cochez PAS** "Initialize with README"
   - Cliquez sur "Create repository"

2. **Connecter votre dépôt local** :
   ```bash
   cd /home/tor/wkspaces/mo2
   git remote add origin https://github.com/VOTRE_USERNAME/cursor.git
   git push -u origin branché-1
   ```
   (Remplacez `VOTRE_USERNAME` par votre nom d'utilisateur GitHub)

3. **Authentification** :
   - Si demandé, utilisez un **Personal Access Token** au lieu du mot de passe
   - Créez un token sur : https://github.com/settings/tokens
   - Scopes nécessaires : `repo` (accès complet aux dépôts)

### Option 2 : Via le script manuel

```bash
cd /home/tor/wkspaces/mo2
./setup_github_manual.sh
```

Le script vous guidera étape par étape.

### Option 3 : Installer GitHub CLI (si vous avez sudo)

```bash
# Installer GitHub CLI
sudo apt update
sudo apt install gh -y

# Se connecter
gh auth login

# Créer le dépôt et pousser
cd /home/tor/wkspaces/mo2
./setup_github.sh
```

## 🔗 GitLens

GitLens fonctionne automatiquement dans VS Code une fois le dépôt connecté à GitHub. Si l'extension n'est pas installée :

1. Ouvrez VS Code
2. Extensions (Ctrl+Shift+X)
3. Recherchez "GitLens"
4. Installez "GitLens — Git supercharged"

## 📊 État actuel

- **Branche active** : `branché-1`
- **Commits** : 1 commit initial
- **Fichiers** : 719 fichiers suivis
- **Remote** : Aucun (à configurer)

Une fois le remote configuré et le push effectué, GitLens affichera automatiquement toutes les informations du dépôt !
