#!/bin/bash
#===============================================================================
#  SCRIPT DE CONFIGURATION SÉCURISÉE DES CLÉS
#  Auteur: Script généré pour Hh
#  Date: $(date +%Y-%m-%d)
#  
#  Ce script:
#  1. Nettoie les anciennes clés SSH
#  2. Génère de nouvelles clés SSH Ed25519
#  3. Crée un fichier .env.local sécurisé (NON versionné)
#  4. Met à jour .gitignore pour protéger les secrets
#  5. Configure les secrets GitHub Actions via gh CLI
#  6. Génère un rapport de log complet
#===============================================================================

set -euo pipefail

#-------------------------------------------------------------------------------
# CONFIGURATION
#-------------------------------------------------------------------------------
REPO_PATH="/home/tor/wkspaces/mo2"
LOG_DIR="${REPO_PATH}/.logs"

# Créer le dossier de logs IMMÉDIATEMENT
mkdir -p "$LOG_DIR"

LOG_FILE="${LOG_DIR}/setup-keys-$(date +%Y%m%d-%H%M%S).log"

# Initialiser le fichier log
touch "$LOG_FILE"
ENV_FILE="${REPO_PATH}/.env.local"
SSH_KEY_PATH="$HOME/.ssh/id_ed25519_mo2"
SSH_EMAIL="alexisdeudon01@gmail.com"
GITHUB_REPO="alexisdeudon01/cursor"

# Couleurs pour l'affichage
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

#-------------------------------------------------------------------------------
# FONCTIONS UTILITAIRES
#-------------------------------------------------------------------------------

# Fonction de logging (écrit dans le fichier ET affiche à l'écran)
log() {
    local level="$1"
    local message="$2"
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    
    # Écrire dans le fichier log (sans couleurs)
    echo "[${timestamp}] [${level}] ${message}" >> "$LOG_FILE"
    
    # Afficher à l'écran (avec couleurs)
    case "$level" in
        "INFO")    echo -e "${BLUE}[INFO]${NC} ${message}" ;;
        "SUCCESS") echo -e "${GREEN}[✅ OK]${NC} ${message}" ;;
        "WARNING") echo -e "${YELLOW}[⚠️ WARN]${NC} ${message}" ;;
        "ERROR")   echo -e "${RED}[❌ ERROR]${NC} ${message}" ;;
        "STEP")    echo -e "${CYAN}[▶️ STEP]${NC} ${message}" ;;
        *)         echo "[${level}] ${message}" ;;
    esac
}

# Fonction pour afficher une bannière
banner() {
    echo ""
    echo -e "${CYAN}============================================================${NC}"
    echo -e "${CYAN}  $1${NC}"
    echo -e "${CYAN}============================================================${NC}"
    echo ""
    log "INFO" "=== $1 ==="
}

# Fonction de confirmation
confirm() {
    local message="$1"
    echo -e "${YELLOW}${message}${NC}"
    read -p "Continuer? (o/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[OoYy]$ ]]; then
        log "INFO" "Opération annulée par l'utilisateur"
        exit 0
    fi
}

#-------------------------------------------------------------------------------
# INITIALISATION
#-------------------------------------------------------------------------------

init_setup() {
    banner "INITIALISATION"
    
    # Le dossier de logs est déjà créé au début du script
    
    # Initialiser le fichier de log avec l'en-tête
    echo "===============================================================================" > "$LOG_FILE"
    echo "  LOG DE CONFIGURATION DES CLÉS SÉCURISÉES" >> "$LOG_FILE"
    echo "  Date: $(date)" >> "$LOG_FILE"
    echo "  Utilisateur: $(whoami)" >> "$LOG_FILE"
    echo "  Machine: $(hostname)" >> "$LOG_FILE"
    echo "===============================================================================" >> "$LOG_FILE"
    echo "" >> "$LOG_FILE"
    
    log "INFO" "Dossier de logs: ${LOG_DIR}"
    log "INFO" "Fichier de log: ${LOG_FILE}"
    log "INFO" "Repo local: ${REPO_PATH}"
    log "INFO" "Repo GitHub: ${GITHUB_REPO}"
    
    # Vérifier que le repo existe
    if [ ! -d "$REPO_PATH" ]; then
        log "ERROR" "Le dossier ${REPO_PATH} n'existe pas!"
        exit 1
    fi
    
    # Vérifier que c'est bien un repo Git
    if [ ! -d "${REPO_PATH}/.git" ]; then
        log "ERROR" "${REPO_PATH} n'est pas un repository Git!"
        exit 1
    fi
    
    log "SUCCESS" "Initialisation terminée"
}

#-------------------------------------------------------------------------------
# ÉTAPE 1: NETTOYAGE DES ANCIENNES CLÉS SSH
#-------------------------------------------------------------------------------

cleanup_ssh_keys() {
    banner "ÉTAPE 1: NETTOYAGE DES CLÉS SSH"
    
    log "STEP" "Recherche des anciennes clés SSH..."
    
    # Liste des fichiers de clés à nettoyer
    local keys_to_clean=(
        "$HOME/.ssh/id_ed25519"
        "$HOME/.ssh/id_ed25519.pub"
        "$HOME/.ssh/id_ed25519_mo2"
        "$HOME/.ssh/id_ed25519_mo2.pub"
        "$HOME/.ssh/id_rsa"
        "$HOME/.ssh/id_rsa.pub"
    )
    
    local cleaned=0
    
    for key in "${keys_to_clean[@]}"; do
        if [ -f "$key" ]; then
            log "INFO" "Suppression de: $key"
            # Backup avant suppression (optionnel)
            mv "$key" "${key}.backup.$(date +%s)" 2>/dev/null || rm -f "$key"
            ((cleaned++)) || true
        fi
    done
    
    if [ $cleaned -eq 0 ]; then
        log "INFO" "Aucune ancienne clé SSH trouvée"
    else
        log "SUCCESS" "${cleaned} fichier(s) de clés nettoyé(s)"
    fi
    
    # Nettoyer aussi les clés connues de l'agent SSH
    log "STEP" "Nettoyage de l'agent SSH..."
    ssh-add -D 2>/dev/null || log "WARNING" "Impossible de nettoyer l'agent SSH (peut-être pas en cours)"
    
    log "SUCCESS" "Nettoyage SSH terminé"
}

#-------------------------------------------------------------------------------
# ÉTAPE 2: GÉNÉRATION DE NOUVELLES CLÉS SSH
#-------------------------------------------------------------------------------

generate_ssh_keys() {
    banner "ÉTAPE 2: GÉNÉRATION DE NOUVELLES CLÉS SSH"
    
    log "STEP" "Génération d'une nouvelle paire de clés Ed25519..."
    
    # Créer le dossier .ssh s'il n'existe pas
    mkdir -p "$HOME/.ssh"
    chmod 700 "$HOME/.ssh"
    
    # Demander la passphrase de manière sécurisée
    echo -e "${YELLOW}Entrez une passphrase pour la nouvelle clé SSH (ou laissez vide):${NC}"
    read -s -p "Passphrase: " SSH_PASSPHRASE
    echo ""
    
    # Générer la clé
    ssh-keygen -t ed25519 -C "$SSH_EMAIL" -f "$SSH_KEY_PATH" -N "$SSH_PASSPHRASE"
    
    if [ $? -eq 0 ]; then
        log "SUCCESS" "Clé SSH générée: ${SSH_KEY_PATH}"
        log "INFO" "Clé publique: ${SSH_KEY_PATH}.pub"
        
        # Afficher la clé publique
        echo ""
        echo -e "${GREEN}=== VOTRE NOUVELLE CLÉ PUBLIQUE SSH ===${NC}"
        echo -e "${CYAN}(À ajouter sur GitHub: Settings → SSH Keys)${NC}"
        echo ""
        cat "${SSH_KEY_PATH}.pub"
        echo ""
        
        # Sauvegarder dans le log (seulement la clé publique, c'est safe)
        log "INFO" "Clé publique générée: $(cat ${SSH_KEY_PATH}.pub)"
        
        # Configurer les permissions
        chmod 600 "$SSH_KEY_PATH"
        chmod 644 "${SSH_KEY_PATH}.pub"
        
        # Ajouter au ssh-agent
        log "STEP" "Ajout de la clé à l'agent SSH..."
        eval "$(ssh-agent -s)" 2>/dev/null || true
        
        if [ -n "$SSH_PASSPHRASE" ]; then
            # Si passphrase, utiliser ssh-add avec expect ou demander manuellement
            log "INFO" "Ajoutez manuellement la clé avec: ssh-add ${SSH_KEY_PATH}"
        else
            ssh-add "$SSH_KEY_PATH" 2>/dev/null && log "SUCCESS" "Clé ajoutée à l'agent SSH"
        fi
        
        # Configurer SSH pour utiliser cette clé pour GitHub
        log "STEP" "Configuration de ~/.ssh/config pour GitHub..."
        
        SSH_CONFIG="$HOME/.ssh/config"
        if ! grep -q "Host github.com" "$SSH_CONFIG" 2>/dev/null; then
            cat >> "$SSH_CONFIG" << EOF

# Configuration pour GitHub (générée automatiquement)
Host github.com
    HostName github.com
    User git
    IdentityFile ${SSH_KEY_PATH}
    IdentitiesOnly yes
EOF
            log "SUCCESS" "Configuration SSH mise à jour"
        else
            log "WARNING" "Configuration GitHub existe déjà dans ~/.ssh/config"
        fi
        
    else
        log "ERROR" "Échec de la génération de la clé SSH"
        exit 1
    fi
}

#-------------------------------------------------------------------------------
# ÉTAPE 3: CRÉATION DU FICHIER .env.local (SÉCURISÉ)
#-------------------------------------------------------------------------------

setup_env_file() {
    banner "ÉTAPE 3: CONFIGURATION DU FICHIER .env.local"
    
    log "STEP" "Création du fichier d'environnement sécurisé..."
    
    # Supprimer l'ancien fichier s'il existe
    if [ -f "$ENV_FILE" ]; then
        log "WARNING" "Fichier .env.local existant trouvé, sauvegarde..."
        mv "$ENV_FILE" "${ENV_FILE}.backup.$(date +%s)"
    fi
    
    # Demander la clé API Anthropic
    echo ""
    echo -e "${YELLOW}============================================================${NC}"
    echo -e "${YELLOW}  CONFIGURATION DE LA CLÉ API ANTHROPIC${NC}"
    echo -e "${YELLOW}============================================================${NC}"
    echo ""
    echo -e "Obtenez une nouvelle clé sur: ${CYAN}https://console.anthropic.com/settings/keys${NC}"
    echo ""
    read -s -p "Collez votre nouvelle clé API Anthropic (sk-ant-...): " ANTHROPIC_KEY
    echo ""
    
    if [ -z "$ANTHROPIC_KEY" ]; then
        log "WARNING" "Aucune clé API fournie, le fichier sera créé avec un placeholder"
        ANTHROPIC_KEY="VOTRE_CLE_API_ICI"
    fi
    
    # Créer le fichier .env.local
    cat > "$ENV_FILE" << EOF
#===============================================================================
# FICHIER D'ENVIRONNEMENT LOCAL - NE JAMAIS COMMITER
# Généré le: $(date)
#===============================================================================

# Clé API Anthropic (Claude)
ANTHROPIC_API_KEY=${ANTHROPIC_KEY}

# Chemin vers la clé SSH (pour référence)
SSH_KEY_PATH=${SSH_KEY_PATH}

#===============================================================================
# INSTRUCTIONS:
# 1. Ce fichier est ignoré par Git (voir .gitignore)
# 2. Pour utiliser ces variables dans un script:
#    source .env.local
# 3. Pour GitHub Actions, utilisez plutôt les secrets GitHub
#===============================================================================
EOF
    
    # Sécuriser les permissions (lecture seule pour le propriétaire)
    chmod 600 "$ENV_FILE"
    
    log "SUCCESS" "Fichier .env.local créé: ${ENV_FILE}"
    log "INFO" "Permissions: 600 (lecture/écriture propriétaire uniquement)"
}

#-------------------------------------------------------------------------------
# ÉTAPE 4: MISE À JOUR DU .gitignore
#-------------------------------------------------------------------------------

update_gitignore() {
    banner "ÉTAPE 4: MISE À JOUR DU .gitignore"
    
    local GITIGNORE="${REPO_PATH}/.gitignore"
    
    log "STEP" "Vérification et mise à jour de .gitignore..."
    
    # Patterns à ignorer pour la sécurité
    local patterns=(
        "# === SÉCURITÉ: Fichiers sensibles ==="
        ".env"
        ".env.*"
        ".env.local"
        "*.pem"
        "*.key"
        "id_ed25519*"
        "id_rsa*"
        "KEYS.txt"
        "secrets.txt"
        "credentials.txt"
        ".logs/"
        "*.backup.*"
    )
    
    # Créer .gitignore s'il n'existe pas
    touch "$GITIGNORE"
    
    local added=0
    for pattern in "${patterns[@]}"; do
        if ! grep -Fxq "$pattern" "$GITIGNORE" 2>/dev/null; then
            echo "$pattern" >> "$GITIGNORE"
            log "INFO" "Ajouté au .gitignore: $pattern"
            ((added++)) || true
        fi
    done
    
    if [ $added -eq 0 ]; then
        log "INFO" "Tous les patterns de sécurité sont déjà présents"
    else
        log "SUCCESS" "${added} pattern(s) ajouté(s) au .gitignore"
    fi
    
    # Vérifier que .env.local n'est pas suivi par Git
    cd "$REPO_PATH"
    if git ls-files --error-unmatch .env.local 2>/dev/null; then
        log "WARNING" ".env.local est suivi par Git! Suppression du suivi..."
        git rm --cached .env.local 2>/dev/null || true
    fi
    
    log "SUCCESS" "Configuration .gitignore terminée"
}

#-------------------------------------------------------------------------------
# ÉTAPE 5: CONFIGURATION DES SECRETS GITHUB
#-------------------------------------------------------------------------------

setup_github_secrets() {
    banner "ÉTAPE 5: CONFIGURATION DES SECRETS GITHUB"
    
    log "STEP" "Vérification de GitHub CLI..."
    
    # Vérifier si gh est installé
    if ! command -v gh &> /dev/null; then
        log "WARNING" "GitHub CLI (gh) n'est pas installé"
        echo ""
        echo -e "${YELLOW}Pour installer GitHub CLI:${NC}"
        echo "  sudo apt install gh"
        echo ""
        echo -e "${YELLOW}Puis relancez ce script ou configurez manuellement:${NC}"
        echo "  gh auth login"
        echo "  gh secret set ANTHROPIC_API_KEY --repo ${GITHUB_REPO}"
        echo ""
        log "INFO" "Instructions d'installation de gh fournies"
        return 0
    fi
    
    log "SUCCESS" "GitHub CLI trouvé: $(gh --version | head -1)"
    
    # Vérifier l'authentification
    log "STEP" "Vérification de l'authentification GitHub..."
    if ! gh auth status &>/dev/null; then
        log "WARNING" "Non authentifié sur GitHub CLI"
        echo ""
        echo -e "${YELLOW}Authentification requise. Exécutez:${NC}"
        echo "  gh auth login"
        echo ""
        
        read -p "Voulez-vous vous authentifier maintenant? (o/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[OoYy]$ ]]; then
            gh auth login
        else
            log "INFO" "Authentification différée"
            return 0
        fi
    fi
    
    log "SUCCESS" "Authentifié sur GitHub"
    
    # Configurer le secret ANTHROPIC_API_KEY
    log "STEP" "Configuration du secret ANTHROPIC_API_KEY sur GitHub..."
    
    # Lire la clé depuis .env.local
    if [ -f "$ENV_FILE" ]; then
        source "$ENV_FILE"
        
        if [ "$ANTHROPIC_API_KEY" != "VOTRE_CLE_API_ICI" ] && [ -n "$ANTHROPIC_API_KEY" ]; then
            echo "$ANTHROPIC_API_KEY" | gh secret set ANTHROPIC_API_KEY --repo "$GITHUB_REPO"
            
            if [ $? -eq 0 ]; then
                log "SUCCESS" "Secret ANTHROPIC_API_KEY configuré sur GitHub"
            else
                log "ERROR" "Échec de la configuration du secret"
            fi
        else
            log "WARNING" "Clé API non configurée dans .env.local"
            echo ""
            echo -e "${YELLOW}Pour configurer le secret manuellement:${NC}"
            echo "  gh secret set ANTHROPIC_API_KEY --repo ${GITHUB_REPO}"
            echo ""
        fi
    fi
    
    # Afficher les secrets existants
    log "STEP" "Liste des secrets configurés..."
    echo ""
    gh secret list --repo "$GITHUB_REPO" 2>/dev/null || log "WARNING" "Impossible de lister les secrets"
    echo ""
    
    # Ajouter la clé SSH publique à GitHub (optionnel)
    echo ""
    read -p "Voulez-vous ajouter la nouvelle clé SSH à GitHub? (o/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[OoYy]$ ]]; then
        local key_title="mo2-$(hostname)-$(date +%Y%m%d)"
        gh ssh-key add "${SSH_KEY_PATH}.pub" --title "$key_title" 2>/dev/null && \
            log "SUCCESS" "Clé SSH ajoutée à GitHub: ${key_title}" || \
            log "WARNING" "Impossible d'ajouter la clé SSH (peut-être déjà existante)"
    fi
}

#-------------------------------------------------------------------------------
# ÉTAPE 6: VÉRIFICATION FINALE
#-------------------------------------------------------------------------------

final_verification() {
    banner "ÉTAPE 6: VÉRIFICATION FINALE"
    
    local errors=0
    
    # Vérifier la clé SSH
    log "STEP" "Vérification de la clé SSH..."
    if [ -f "$SSH_KEY_PATH" ] && [ -f "${SSH_KEY_PATH}.pub" ]; then
        log "SUCCESS" "Clé SSH présente: ${SSH_KEY_PATH}"
    else
        log "ERROR" "Clé SSH manquante!"
        ((errors++)) || true
    fi
    
    # Vérifier le fichier .env.local
    log "STEP" "Vérification du fichier .env.local..."
    if [ -f "$ENV_FILE" ]; then
        local perms=$(stat -c %a "$ENV_FILE")
        if [ "$perms" == "600" ]; then
            log "SUCCESS" "Fichier .env.local présent avec bonnes permissions (600)"
        else
            log "WARNING" "Permissions de .env.local: $perms (devrait être 600)"
            chmod 600 "$ENV_FILE"
        fi
    else
        log "ERROR" "Fichier .env.local manquant!"
        ((errors++)) || true
    fi
    
    # Vérifier le .gitignore
    log "STEP" "Vérification du .gitignore..."
    if grep -q ".env.local" "${REPO_PATH}/.gitignore" 2>/dev/null; then
        log "SUCCESS" ".env.local est ignoré par Git"
    else
        log "ERROR" ".env.local n'est pas dans .gitignore!"
        ((errors++)) || true
    fi
    
    # Test de connexion SSH à GitHub
    log "STEP" "Test de connexion SSH à GitHub..."
    ssh -T git@github.com -o BatchMode=yes -o StrictHostKeyChecking=no 2>&1 | head -1 || true
    # Note: Cette commande retourne toujours une erreur (GitHub n'autorise pas le shell)
    # mais le message indique si l'authentification a réussi
    
    # Résumé final
    echo ""
    echo -e "${CYAN}============================================================${NC}"
    echo -e "${CYAN}  RÉSUMÉ DE LA CONFIGURATION${NC}"
    echo -e "${CYAN}============================================================${NC}"
    echo ""
    echo -e "  📁 Repo local:        ${REPO_PATH}"
    echo -e "  🔑 Clé SSH:           ${SSH_KEY_PATH}"
    echo -e "  📄 Fichier .env:      ${ENV_FILE}"
    echo -e "  📋 Fichier de log:    ${LOG_FILE}"
    echo -e "  🐙 Repo GitHub:       ${GITHUB_REPO}"
    echo ""
    
    if [ $errors -eq 0 ]; then
        echo -e "${GREEN}  ✅ CONFIGURATION RÉUSSIE${NC}"
        log "SUCCESS" "Configuration terminée sans erreurs"
    else
        echo -e "${RED}  ❌ ${errors} ERREUR(S) DÉTECTÉE(S)${NC}"
        log "ERROR" "Configuration terminée avec ${errors} erreur(s)"
    fi
    
    echo ""
    echo -e "${CYAN}============================================================${NC}"
    echo -e "${CYAN}  PROCHAINES ÉTAPES${NC}"
    echo -e "${CYAN}============================================================${NC}"
    echo ""
    echo -e "  1. ${YELLOW}Ajoutez la clé SSH publique sur GitHub:${NC}"
    echo -e "     https://github.com/settings/ssh/new"
    echo ""
    echo -e "  2. ${YELLOW}Vérifiez les secrets GitHub Actions:${NC}"
    echo -e "     https://github.com/${GITHUB_REPO}/settings/secrets/actions"
    echo ""
    echo -e "  3. ${YELLOW}Relancez le workflow:${NC}"
    echo -e "     gh workflow run \"Auto-Improve Project (Thebestclient)\" --ref dev"
    echo ""
    echo -e "  4. ${YELLOW}Consultez les logs:${NC}"
    echo -e "     cat ${LOG_FILE}"
    echo ""
}

#-------------------------------------------------------------------------------
# MAIN
#-------------------------------------------------------------------------------

main() {
    clear
    echo ""
    echo -e "${CYAN}╔══════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${CYAN}║                                                              ║${NC}"
    echo -e "${CYAN}║   🔐 SCRIPT DE CONFIGURATION SÉCURISÉE DES CLÉS 🔐          ║${NC}"
    echo -e "${CYAN}║                                                              ║${NC}"
    echo -e "${CYAN}║   Ce script va:                                              ║${NC}"
    echo -e "${CYAN}║   • Nettoyer les anciennes clés SSH                          ║${NC}"
    echo -e "${CYAN}║   • Générer de nouvelles clés Ed25519                        ║${NC}"
    echo -e "${CYAN}║   • Créer un fichier .env.local sécurisé                     ║${NC}"
    echo -e "${CYAN}║   • Mettre à jour .gitignore                                 ║${NC}"
    echo -e "${CYAN}║   • Configurer les secrets GitHub Actions                    ║${NC}"
    echo -e "${CYAN}║                                                              ║${NC}"
    echo -e "${CYAN}╚══════════════════════════════════════════════════════════════╝${NC}"
    echo ""
    
    confirm "⚠️  Ce script va modifier vos clés SSH et fichiers de configuration."
    
    # Exécuter les étapes
    init_setup
    cleanup_ssh_keys
    generate_ssh_keys
    setup_env_file
    update_gitignore
    setup_github_secrets
    final_verification
    
    echo ""
    log "INFO" "Script terminé à $(date)"
    echo ""
}

# Exécuter le script
main "$@"
