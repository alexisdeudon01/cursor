# Instructions pour configurer GitHub

## ✅ Déjà fait
- ✅ Dépôt Git initialisé
- ✅ Branche `branché-1` créée
- ✅ `.gitignore` mis à jour avec les règles Unity complètes

## 📋 Étapes restantes

### 1. Installer GitHub CLI (si pas déjà installé)

Exécutez ces commandes dans votre terminal :

```bash
curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg
sudo chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null
sudo apt update
sudo apt install gh -y
```

**Alternative** : Visitez https://github.com/cli/cli/blob/trunk/docs/install_linux.md pour d'autres méthodes d'installation.

### 2. Se connecter à GitHub

```bash
gh auth login
```

Suivez les instructions :
- Choisissez `GitHub.com`
- Choisissez votre méthode d'authentification (HTTPS ou SSH)
- Suivez les étapes pour vous authentifier

### 3. Exécuter le script de configuration

```bash
cd /home/tor/wkspaces/mo2
./setup_github.sh
```

Ce script va :
- Vérifier que GitHub CLI est installé et que vous êtes connecté
- Créer le dépôt `cursor` sur votre compte GitHub
- Ajouter tous les fichiers
- Faire un commit initial
- Pousser la branche `branché-1` vers GitHub

### 4. GitLens dans VS Code

GitLens est une extension VS Code qui fonctionne automatiquement avec les dépôts Git. Une fois le dépôt poussé vers GitHub :

1. Ouvrez le projet dans VS Code
2. GitLens détectera automatiquement le dépôt Git
3. Vous verrez l'historique des commits, les branches, et toutes les fonctionnalités GitLens

Si GitLens n'est pas installé :
- Ouvrez VS Code
- Allez dans Extensions (Ctrl+Shift+X)
- Recherchez "GitLens"
- Installez l'extension "GitLens — Git supercharged"

## 🔧 Commandes manuelles (si le script ne fonctionne pas)

Si vous préférez faire les étapes manuellement :

```bash
# 1. Créer le dépôt sur GitHub (via l'interface web ou CLI)
gh repo create cursor --public

# 2. Ajouter le remote
git remote add origin https://github.com/VOTRE_USERNAME/cursor.git

# 3. Ajouter tous les fichiers
git add .

# 4. Faire un commit
git commit -m "Initial commit: Unity multiplayer game project"

# 5. Pousser la branche
git push -u origin branché-1
```

## 📝 Notes

- La branche `branché-1` est maintenant votre branche active
- Le `.gitignore` inclut toutes les règles Unity standard pour ignorer les fichiers générés
- GitLens fonctionnera automatiquement une fois le dépôt connecté à GitHub
