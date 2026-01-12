# Project Tools - Menu Unity Editor

## 📍 Emplacement du menu

Le menu devrait apparaître dans Unity Editor sous :
- **`Tools > Push to branché-1 (avec vérification)`**
- **`Tools > Push to dev`**
- **`Tools > Project Tools > ...`** (sous-menu avec tous les outils)

## 🔧 Si le menu n'apparaît pas

### 1. Vérifier que le fichier compile
- Ouvrez la Console Unity (`Window > General > Console`)
- Vérifiez qu'il n'y a pas d'erreurs de compilation
- Si erreurs, corrigez-les et attendez la recompilation

### 2. Forcer la recompilation
- Dans Unity : `Assets > Reimport All`
- Ou fermez et rouvrez Unity

### 3. Vérifier le fichier
- Le fichier doit être dans `Assets/Editor/ProjectTools.cs`
- Il doit avoir un fichier `.meta` associé
- Le namespace a été retiré (classe globale)

### 4. Vérifier Unity 6000.3.0f1
- Le code utilise `System.Diagnostics` qui est disponible
- Les directives `#if UNITY_EDITOR_LINUX` sont correctes pour Linux

## 📋 Boutons disponibles

### Dans `Tools` (direct)
- **Push to branché-1 (avec vérification)** - Exécute `push_to_branche1.sh` avec vérification Git
- **Push to dev** - Exécute `push_to_dev.sh` avec vérification Git

### Dans `Tools > Project Tools`
- **Export > Class Diagram**
- **Export > DTO Diagram**
- **Export > Package Diagram**
- **Git > Push to branché-1**
- **Git > Push to dev**
- **Errors > Check & Fix All**
- **Errors > Fix Assembly Definitions**
- **Errors > Check Input Manager**
- **Errors > Check GridMapExporter**

## 🐛 Dépannage

Si le menu n'apparaît toujours pas :

1. **Vérifier les erreurs de compilation** :
   ```
   Console Unity > Chercher "ProjectTools"
   ```

2. **Vérifier que le script est dans Editor** :
   ```
   Assets/Editor/ProjectTools.cs doit exister
   ```

3. **Supprimer les fichiers .meta et reimporter** :
   - Supprimez `Assets/Editor/ProjectTools.cs.meta`
   - Dans Unity : `Assets > Reimport All`

4. **Vérifier les permissions** :
   - Le fichier doit être lisible
   - Les scripts bash doivent être exécutables

## ✅ Test rapide

Pour tester si le menu fonctionne :
1. Ouvrez Unity Editor
2. Allez dans `Tools`
3. Cherchez "Push to branché-1" ou "Push to dev"
4. Si visible, cliquez dessus pour tester
