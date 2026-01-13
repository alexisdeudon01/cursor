# cursor

## Liste des fichiers

Ce dépôt contient un petit script pour **lister les fichiers** du projet en s’appuyant sur `git` (donc en respectant automatiquement `.gitignore`).

### Utilisation

- Liste simple (fichiers trackés par git):

```bash
python3 scripts/liste_fichiers.py
```

- Inclure aussi les fichiers non trackés (en respectant `.gitignore`):

```bash
python3 scripts/liste_fichiers.py --all
```

- Afficher une arborescence:

```bash
python3 scripts/liste_fichiers.py --tree
```
