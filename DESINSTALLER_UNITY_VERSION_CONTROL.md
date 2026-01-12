# Désinstaller Unity Version Control (Plastic SCM)

## ✅ Oui, désinstaller le package fonctionne aussi !

Désinstaller le package Unity Version Control est **la meilleure méthode** pour s'assurer qu'il ne sera plus utilisé.

## Méthodes pour désinstaller

### Méthode 1 : Via Unity Package Manager (Recommandé)

1. **Ouvrez Unity Editor**
2. **Allez dans** : `Window` → `Package Manager`
3. **Dans le menu déroulant en haut à gauche**, sélectionnez **"In Project"** ou **"My Registries"**
4. **Recherchez** : "Plastic SCM" ou "Version Control"
5. **Cliquez sur le package** et sélectionnez **"Remove"** ou **"Uninstall"**

### Méthode 2 : Via manifest.json (Manuel)

Si le package est dans `Packages/manifest.json`, supprimez la ligne correspondante :

```json
{
  "dependencies": {
    // Supprimez cette ligne si présente :
    "com.unity.plastic": "x.x.x",
    // ou
    "com.unity.collab-proxy": "x.x.x"
  }
}
```

Puis Unity rechargera automatiquement les packages.

### Méthode 3 : Via Unity Hub

1. **Ouvrez Unity Hub**
2. **Allez dans** : `Installs` → Sélectionnez votre version Unity
3. **Cliquez sur** : `Add modules` ou `Modules`
4. **Décochez** : "Plastic SCM" ou "Version Control"
5. **Appliquer les changements**

## Vérification

Après désinstallation, vérifiez que :
- ✅ Le package n'apparaît plus dans Package Manager
- ✅ Les fichiers `.plastic/` ne sont plus créés (déjà dans `.gitignore`)
- ✅ Le menu `Assets` → `Plastic SCM` n'apparaît plus

## 📋 État dans votre projet

✅ **Package détecté** : `com.unity.collab-proxy` version 2.10.2

⚠️ **Important** : Ce package est une **dépendance** de `com.unity.services.cloud-build` (installé dans votre projet).

### ⚠️ Attention

Si vous désinstallez `com.unity.collab-proxy`, vous devrez aussi désinstaller `com.unity.services.cloud-build` qui en dépend.

## 🎯 Solutions recommandées

### Option 1 : Désactiver Unity Version Control (Recommandé)

**Vous n'avez pas besoin de désinstaller le package !** Il suffit de désactiver l'intégration :

1. **Unity Editor** → `Edit` → `Preferences` → `Version Control`
2. **Désactivez** "Enable Version Control Integration"
3. Ou : `Assets` → `Version Control` → `Disable Version Control`

Les fichiers `.plastic/` sont déjà ignorés par `.gitignore`, donc même si le package est installé, il ne créera plus de fichiers dans votre dépôt Git.

### Option 2 : Désinstaller le package (si vous n'utilisez pas Cloud Build)

Si vous n'utilisez pas `com.unity.services.cloud-build`, vous pouvez :

1. **Désinstaller** `com.unity.services.cloud-build` depuis Package Manager
2. Cela désinstallera automatiquement `com.unity.collab-proxy`

**Note** : Cette action supprimera aussi les fonctionnalités Cloud Build d'Unity.
