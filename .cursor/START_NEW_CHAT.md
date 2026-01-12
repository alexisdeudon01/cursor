# Comment démarrer un nouveau chat avec l'agent

## Méthode 1 : Nouveau chat avec référence à l'agent

1. **Ouvrez un nouveau chat** dans Cursor :
   - `Ctrl+L` (ou `Cmd+L` sur Mac) pour ouvrir le chat
   - Ou cliquez sur l'icône de chat dans la barre latérale

2. **Dans le nouveau chat, tapez en premier :**
   ```
   @cursor-ngo-dedicated-server
   ```

3. **Puis votre question ou tâche**, par exemple :
   ```
   @cursor-ngo-dedicated-server
   
   Peux-tu analyser l'architecture actuelle et proposer les prochaines étapes ?
   ```

## Méthode 2 : Utiliser le raccourci direct

1. Ouvrez le chat (`Ctrl+L`)
2. Commencez directement par `@cursor-ngo-dedicated-server` suivi de votre question
3. L'agent sera automatiquement chargé

## Méthode 3 : Via la commande @ dans n'importe quel chat

À tout moment dans un chat Cursor, vous pouvez référencer l'agent :

```
@cursor-ngo-dedicated-server [votre question]
```

## Exemples de prompts pour démarrer

### Exemple 1 : Analyse de l'architecture
```
@cursor-ngo-dedicated-server

Analyse l'état actuel de l'implémentation et identifie ce qui reste à faire selon le plan.
```

### Exemple 2 : Implémentation
```
@cursor-ngo-dedicated-server

Continue l'implémentation de la Phase 2 : créer les DTOs partagés pour les messages réseau.
```

### Exemple 3 : Validation
```
@cursor-ngo-dedicated-server

Valide que SessionContainer implémente correctement la FSM selon l'architecture cible.
```

## Note importante

- L'agent reste actif pour toute la conversation une fois référencé avec `@`
- Vous pouvez utiliser `@cursor-ngo-dedicated-server` dans n'importe quel nouveau chat
- L'agent suivra automatiquement les directives définies dans `.cursor/agents/cursor-ngo-dedicated-server.md`

## Vérification

Pour vérifier que l'agent est actif, demandez-lui :
```
@cursor-ngo-dedicated-server

Quelle est l'architecture cible de ce projet ?
```

L'agent devrait répondre avec les détails de l'architecture Unity NGO Dedicated Server.
