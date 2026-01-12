# Système d'Amélioration Continue avec IA

Ce système utilise **l'IA Claude (moi)** pour analyser et améliorer automatiquement le projet toutes les 30 minutes.

## Comment ça fonctionne

### Avec IA (recommandé)
1. **GitHub Actions** s'exécute toutes les 30 minutes
2. **Script Python** appelle l'API Claude (Anthropic)
3. **Moi (l'IA)** analyse le codebase et propose des améliorations
4. **Les améliorations critiques** sont appliquées automatiquement
5. **Commit et push** automatiques sur la branche `dev`

### Sans IA (fallback)
Si `ANTHROPIC_API_KEY` n'est pas configuré, le système utilise un script Python basique qui fait seulement des vérifications simples.

## Configuration requise

### 1. Créer une clé API Anthropic
1. Aller sur https://console.anthropic.com/
2. Créer une clé API
3. Copier la clé

### 2. Ajouter le secret dans GitHub
1. Aller dans ton repo GitHub
2. **Settings** → **Secrets and variables** → **Actions**
3. Cliquer **New repository secret**
4. Nom: `ANTHROPIC_API_KEY`
5. Valeur: ta clé API Anthropic
6. Cliquer **Add secret**

### 3. Activer le workflow
Le workflow est déjà activé par défaut. Il s'exécutera automatiquement toutes les 30 minutes.

## Ce que l'IA fait

### Analyse
- ✅ Architecture (séparation Client/Serveur)
- ✅ Modularité (jeux, sessions, maps)
- ✅ Configuration réseau
- ✅ Problèmes de code

### Améliorations
- ✅ Applique les changements critiques automatiquement
- ✅ Crée des patches pour les améliorations importantes
- ✅ Met à jour la documentation
- ✅ Crée de nouvelles versions de l'agent

## Fichiers générés

- `.cursor/agents/thebestclientX.md` - Nouvelle version de l'agent
- `.cursor/agents/thebestclientX-analysis-report.md` - Rapport d'analyse
- `.cursor/agents/review-playbook-vX.md` - Playbook mis à jour
- `.cursor/agents/improvement-log.md` - Journal des améliorations

## Coût API

L'API Claude est payante mais très raisonnable:
- ~$0.003 par requête (analyse complète)
- 48 requêtes/jour = ~$0.14/jour
- ~$4.20/mois pour amélioration continue 24/7

## Vérification

Pour vérifier que l'IA fonctionne:
1. Aller dans **Actions** sur GitHub
2. Vérifier que le workflow `Auto-Improve Project (Thebestclient)` s'exécute
3. Regarder les logs pour voir "🤖 Utilisation de l'IA Claude..."

## Désactiver temporairement

Pour désactiver temporairement:
1. Aller dans **Actions** → **Workflows**
2. Trouver `Auto-Improve Project (Thebestclient)`
3. Cliquer **...** → **Disable workflow**

Ou simplement retirer `ANTHROPIC_API_KEY` des secrets (le système passera en mode basique).
