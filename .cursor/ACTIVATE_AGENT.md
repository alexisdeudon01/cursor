# Comment activer l'agent dans Cursor 2.3.34

## Méthode 1 : Via la commande @ (Recommandé)

Dans Cursor, utilisez directement la commande `@` pour référencer l'agent :

1. Ouvrez le chat Cursor (`Ctrl+L` ou `Cmd+L`)
2. Tapez : `@cursor-ngo-dedicated-server` ou `@.cursor/agents/cursor-ngo-dedicated-server.md`
3. L'agent sera chargé et utilisé pour la conversation

## Méthode 2 : Via les paramètres Cursor

1. Ouvrez les paramètres : `Ctrl+,` (ou `Cmd+,` sur Mac)
2. Recherchez "agent" ou "custom agent"
3. Dans "Custom Agents" ou "Agent Settings", ajoutez le chemin :
   - `.cursor/agents/cursor-ngo-dedicated-server.md`
   - Ou le chemin absolu : `/home/tor/wkspaces/mo2/.cursor/agents/cursor-ngo-dedicated-server.md`

## Méthode 3 : Via la palette de commandes

1. Ouvrez la palette : `Ctrl+Shift+P` (ou `Cmd+Shift+P`)
2. Tapez : "agent" ou "select agent"
3. Si l'option apparaît, sélectionnez votre agent

## Méthode 4 : Utilisation directe dans le code

Vous pouvez aussi référencer l'agent directement dans vos prompts :

```
Utilise l'agent cursor-ngo-dedicated-server pour cette tâche.
```

Ou dans le chat Cursor, commencez par :
```
@.cursor/agents/cursor-ngo-dedicated-server.md

[Votre question ou tâche ici]
```

## Vérification

Pour vérifier que l'agent est bien détecté :

1. Ouvrez le chat Cursor
2. Tapez : `@cursor-ngo-dedicated-server help`
3. Si l'agent répond avec sa description, il est bien chargé

## Note importante

Dans Cursor 2.3.34, les agents personnalisés peuvent ne pas apparaître automatiquement dans une liste d'agents. Ils doivent être référencés explicitement via la commande `@` ou configurés dans les paramètres.

## Fichier de l'agent

L'agent se trouve à :
- Chemin relatif : `.cursor/agents/cursor-ngo-dedicated-server.md`
- Chemin absolu : `/home/tor/wkspaces/mo2/.cursor/agents/cursor-ngo-dedicated-server.md`
- Nom : "Unity NGO Dedicated Server (2D) - Client/Server Dual Build"
