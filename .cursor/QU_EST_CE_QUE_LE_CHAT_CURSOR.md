# Qu'est-ce que le Chat Cursor ?

Le **Chat Cursor** est l'interface de conversation avec l'assistant IA intégré dans l'éditeur Cursor. C'est là que vous pouvez poser des questions, demander de l'aide pour le code, et utiliser les agents personnalisés.

## 🎯 Comment ouvrir le Chat Cursor

### Méthode 1 : Raccourci clavier (le plus rapide)
- **Linux/Windows** : `Ctrl + L` ou `Ctrl + K`
- **Mac** : `Cmd + L` ou `Cmd + K`

### Méthode 2 : Via l'interface
1. Regardez en bas à droite de la fenêtre Cursor
2. Vous devriez voir une icône de chat ou une barre de recherche
3. Cliquez dessus pour ouvrir le chat

### Méthode 3 : Via le menu
1. Allez dans le menu **View** (ou **Affichage**)
2. Cherchez **"Chat"** ou **"AI Chat"**
3. Cliquez pour ouvrir

## 💬 Comment utiliser le Chat

### Interface du chat
Une fois ouvert, vous verrez :
- Une zone de texte en bas pour taper vos questions
- L'historique de la conversation au-dessus
- Des boutons pour envoyer, effacer, etc.

### Utiliser un agent dans le chat

Pour utiliser l'agent `cursor-ngo-dedicated-server`, tapez dans le chat :

```
@.cursor/agents/cursor-ngo-dedicated-server.md

[Votre question ici]
```

Ou simplement :

```
@cursor-ngo-dedicated-server

[Votre question ici]
```

## 📝 Exemples d'utilisation

### Exemple 1 : Demander de l'aide
```
Comment créer une nouvelle session de jeu ?
```

### Exemple 2 : Avec l'agent
```
@cursor-ngo-dedicated-server

Comment implémenter SessionContainer selon l'architecture ?
```

### Exemple 3 : Demander une explication
```
Explique-moi comment fonctionne GameInstanceManager
```

## 🔍 Fonctionnalités du Chat

1. **Réponses contextuelles** : Le chat comprend le contexte de votre projet
2. **Références au code** : Il peut citer des fichiers et lignes spécifiques
3. **Suggestions de code** : Il peut proposer du code directement
4. **Agents personnalisés** : Vous pouvez charger des agents spécialisés

## ⚙️ Configuration

Si le chat ne s'ouvre pas :
1. Vérifiez que vous êtes connecté à Cursor (compte nécessaire)
2. Vérifiez votre connexion internet
3. Redémarrez Cursor si nécessaire

## 🎓 Astuces

- Utilisez `Ctrl+L` pour ouvrir rapidement le chat
- Vous pouvez référencer des fichiers avec `@nom-du-fichier`
- Vous pouvez référencer des agents avec `@nom-de-l-agent`
- Le chat garde l'historique de votre conversation

## 📚 Ressources

- Documentation Cursor : https://cursor.sh/docs
- Guide des agents : `.cursor/HOW_TO_USE_AGENT.md`
- Guide d'activation : `.cursor/ACTIVATE_AGENT.md`
