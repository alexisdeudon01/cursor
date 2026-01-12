# 🧹 Nettoyage de la clé API de l'historique Git

## ⚠️ IMPORTANT: Ne PAS bypasser la protection GitHub !

Bypasser la protection expose ta clé API publiquement. Au lieu de ça, nettoyons l'historique Git.

## Solution: Nettoyer l'historique Git

### Option 1: Utiliser git filter-repo (Recommandé)

```bash
# Installer git-filter-repo si nécessaire
pip install git-filter-repo

# Nettoyer l'historique
git filter-repo --path-glob '.github/**' --invert-paths --force
# Puis réécrire les fichiers sans la clé
```

### Option 2: Créer une nouvelle branche propre

```bash
# Créer une nouvelle branche depuis un commit avant la clé
git checkout -b dev-clean <commit-avant-la-cle>
# Copier les fichiers modifiés (sans la clé)
# Commit et push
```

### Option 3: Utiliser BFG Repo-Cleaner

```bash
# Télécharger BFG
# Créer un fichier secrets.txt avec la clé
echo "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" > secrets.txt
# Nettoyer
java -jar bfg.jar --replace-text secrets.txt
git reflog expire --expire=now --all && git gc --prune=now --aggressive
```

## Solution Simple: Réécrire les commits problématiques

Le commit problématique est `0699ff9500ed5cc0cf5de1728690507f6f2014ec`.

On peut utiliser `git rebase -i` pour modifier ce commit:

```bash
# Trouver le commit avant le problème
git log --oneline | grep -B5 "0699ff9"

# Rebase interactif
git rebase -i <commit-avant>

# Dans l'éditeur, changer "pick" en "edit" pour le commit problématique
# Puis modifier les fichiers pour retirer la clé
# git commit --amend
# git rebase --continue
```

## Solution la PLUS SIMPLE: Nouvelle branche

1. **Créer une nouvelle branche propre**:
```bash
git checkout -b dev-clean origin/dev~5  # 5 commits avant le problème
```

2. **Copier les fichiers modifiés** (sans la clé):
```bash
# Les fichiers sont déjà propres maintenant
git add .
git commit -m "🧹 Branche propre sans clé API"
```

3. **Remplacer dev par dev-clean**:
```bash
git push origin dev-clean:dev --force
```

## Après nettoyage

Une fois l'historique nettoyé:
1. Ajouter la clé dans GitHub Secrets (interface web)
2. Le système fonctionnera automatiquement

---

**Recommandation**: Utiliser la solution "Nouvelle branche" - c'est la plus simple et sûre.
