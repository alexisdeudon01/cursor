# Diagrammes UML

Ce dossier contient tous les diagrammes UML générés automatiquement à chaque itération d'amélioration continue.

## Structure

- `architecture-vX.mmd` - Diagramme d'architecture (Mermaid)
- `architecture-vX.png` - Diagramme d'architecture (PNG)
- `modularity-vX.mmd` - Diagramme de modularité (Mermaid)
- `modularity-vX.png` - Diagramme de modularité (PNG)
- `client-server-vX.mmd` - Diagramme Client/Serveur (Mermaid)
- `client-server-vX.png` - Diagramme Client/Serveur (PNG)
- `diagrams-vX.md` - Résumé des diagrammes pour la version X

## Génération automatique

Les diagrammes sont générés automatiquement:
- À chaque cycle d'amélioration continue (toutes les 30 minutes)
- Via le script `.github/scripts/generate-uml-diagrams.py`
- Dans le workflow GitHub Actions

## Formats

- **Mermaid (.mmd)**: Format source, éditable
- **PNG (.png)**: Format image pour visualisation
- **Markdown (.md)**: Résumé avec liens vers les diagrammes

## Visualisation

Les diagrammes Mermaid peuvent être visualisés:
- Directement dans GitHub (Markdown les rend automatiquement)
- Avec l'extension Mermaid pour VS Code
- Sur https://mermaid.live/

Les PNG peuvent être visualisés dans n'importe quel visualiseur d'images.
