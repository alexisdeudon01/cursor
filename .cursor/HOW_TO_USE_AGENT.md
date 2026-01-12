# Comment utiliser l'agent dans Cursor

## ⚠️ Si `@cursor-ngo-dedicated-server` ne fonctionne pas

Cursor peut avoir des difficultés à détecter automatiquement les agents. Voici plusieurs méthodes pour utiliser l'agent :

## Méthode 1 : Référence directe du fichier (Recommandé)

Dans le chat Cursor, utilisez le chemin complet :

```
@.cursor/agents/cursor-ngo-dedicated-server.md
```

Ou le chemin absolu :

```
@/home/tor/wkspaces/mo2/.cursor/agents/cursor-ngo-dedicated-server.md
```

## Méthode 2 : Mention dans le prompt

Commencez votre message par :

```
Utilise l'agent défini dans .cursor/agents/cursor-ngo-dedicated-server.md

[Votre question ici]
```

## Méthode 3 : Copier le contenu dans le prompt

Si les méthodes précédentes ne fonctionnent pas, vous pouvez copier les directives importantes de l'agent directement dans votre prompt :

```
Je travaille sur un projet Unity 2D avec Netcode for GameObjects.
Architecture: serveur dédié autoritaire, builds client/serveur séparés.
Référence: .cursor/agents/cursor-ngo-dedicated-server.md

[Votre question ici]
```

## Méthode 4 : Configuration dans les paramètres Cursor

1. Ouvrez les paramètres Cursor : `Ctrl+,` (ou `Cmd+,`)
2. Recherchez "Rules" ou "Custom Instructions"
3. Ajoutez le chemin vers l'agent dans les règles du projet

## Vérification

Pour vérifier que l'agent est bien chargé, demandez :

```
Quel est le rôle de l'agent cursor-ngo-dedicated-server ?
```

Si l'agent répond avec sa description, il est bien chargé.

## Note importante

Dans certaines versions de Cursor, les agents personnalisés doivent être référencés explicitement via le chemin du fichier plutôt que par leur nom seul.
