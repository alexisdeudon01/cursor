# Instructions pour désactiver Unity Version Control dans Unity Editor

Pour désactiver complètement Unity Version Control (Plastic SCM) dans Unity :

1. **Ouvrez Unity Editor**
2. **Allez dans** : `Edit` → `Preferences` (ou `Unity` → `Preferences` sur Mac)
3. **Dans la section "Version Control"** :
   - Décochez "Enable Version Control Integration" si présent
   - Changez le mode de version control si nécessaire
4. **Ou via le menu** :
   - `Assets` → `Version Control` → `Disable Version Control`
   - Ou `Assets` → `Plastic SCM` → `Disable Plastic SCM`

**Note** : Les fichiers `.plastic/` ont déjà été supprimés du dépôt Git et ajoutés au `.gitignore`. Unity ne devrait plus créer ces fichiers automatiquement.
