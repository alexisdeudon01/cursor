# Workflow de développement avec branche dev

## 🌿 Structure des branches

- **`branché-1`** : Branche principale (production)
- **`dev`** : Branche de développement (travail en cours)

## 📋 Workflow

### 1. Travailler sur la branche dev

```bash
# Basculer vers dev
git checkout dev

# Faire vos modifications
# ...

# Commiter vos changements
git add .
git commit -m "Description des modifications"

# Pousser vers dev
git push origin dev
```

### 2. Merger dev vers branché-1

Quand vous êtes prêt à merger vos modifications dans la branche principale :

#### Option 1 : Script automatique (Recommandé)

```bash
./merge_dev_to_main.sh
```

Le script va :
- Vérifier que vous êtes sur dev
- S'assurer que tout est commité
- Basculer vers branché-1
- Merger dev dans branché-1
- Vous proposer de pousser vers GitHub

#### Option 2 : Commandes manuelles

```bash
# 1. S'assurer que dev est à jour et tout est commité
git checkout dev
git status

# 2. Basculer vers branché-1
git checkout branché-1

# 3. Merger dev
git merge dev --no-ff -m "Merge dev into branché-1"

# 4. Résoudre les conflits si nécessaire
# git add <fichiers-résolus>
# git commit

# 5. Pousser vers GitHub
git push origin branché-1
```

## 🔄 Synchronisation

### Mettre à jour dev depuis branché-1

Si branché-1 a été mis à jour et que vous voulez synchroniser dev :

```bash
git checkout dev
git merge branché-1
```

### Mettre à jour depuis GitHub

```bash
# Pour dev
git checkout dev
git pull origin dev

# Pour branché-1
git checkout branché-1
git pull origin branché-1
```

## 📝 Bonnes pratiques

1. **Toujours travailler sur dev** pour les nouvelles fonctionnalités et corrections
2. **Tester sur dev** avant de merger vers branché-1
3. **Merger régulièrement** pour éviter les gros conflits
4. **Utiliser des messages de commit clairs** pour faciliter le suivi

## ⚠️ En cas de conflits

Si vous avez des conflits lors du merge :

1. Ouvrez les fichiers en conflit
2. Résolvez les conflits manuellement
3. Marquez les fichiers comme résolus : `git add <fichier>`
4. Finalisez le merge : `git commit`

## 🎯 État actuel

- ✅ Branche `dev` créée
- ✅ Modifications fixes commitées dans dev
- ✅ Script de merge disponible : `merge_dev_to_main.sh`
