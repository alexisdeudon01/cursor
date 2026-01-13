#!/bin/bash
# Script de build Unity pour Client et Serveur
# Utilisé dans Docker Unity 6000.3.0f1

set -e

UNITY_VERSION="6000.3.0f1"
PROJECT_PATH="/workspace"
BUILD_CLIENT_PATH="${PROJECT_PATH}/Build/Client"
BUILD_SERVER_PATH="${PROJECT_PATH}/Build/Server"

echo "🔨 Build Unity ${UNITY_VERSION}"
echo "=================================="
echo ""

# Vérifier que Unity est disponible
if [ ! -f "/opt/unity/Editor/Unity" ]; then
    echo "❌ Unity Editor non trouvé"
    exit 1
fi

# Fonction pour build Client
build_client() {
    echo "📦 Build Client..."
    echo "   Scènes: Menu, Client, Game"
    echo "   Target: StandaloneLinux64"
    echo ""
    
    /opt/unity/Editor/Unity \
        -batchmode \
        -quit \
        -projectPath "${PROJECT_PATH}" \
        -buildTarget Linux64 \
        -buildLinux64Player "${BUILD_CLIENT_PATH}/Client.x86_64" \
        -executeMethod BuildScript.BuildClient \
        -logFile "${PROJECT_PATH}/build-client.log" \
        || {
            echo "❌ Erreur build Client"
            cat "${PROJECT_PATH}/build-client.log" | tail -50
            exit 1
        }
    
    if [ -f "${BUILD_CLIENT_PATH}/Client.x86_64" ]; then
        echo "✅ Build Client réussi: ${BUILD_CLIENT_PATH}/Client.x86_64"
        chmod +x "${BUILD_CLIENT_PATH}/Client.x86_64"
    else
        echo "❌ Fichier Client.x86_64 non trouvé"
        exit 1
    fi
}

# Fonction pour build Serveur
build_server() {
    echo "📦 Build Serveur..."
    echo "   Scène: Server"
    echo "   Target: LinuxServer"
    echo ""
    
    /opt/unity/Editor/Unity \
        -batchmode \
        -quit \
        -projectPath "${PROJECT_PATH}" \
        -buildTarget LinuxServer \
        -buildLinuxServerPlayer "${BUILD_SERVER_PATH}/Server.x86_64" \
        -executeMethod BuildScript.BuildServer \
        -logFile "${PROJECT_PATH}/build-server.log" \
        || {
            echo "❌ Erreur build Serveur"
            cat "${PROJECT_PATH}/build-server.log" | tail -50
            exit 1
        }
    
    if [ -f "${BUILD_SERVER_PATH}/Server.x86_64" ]; then
        echo "✅ Build Serveur réussi: ${BUILD_SERVER_PATH}/Server.x86_64"
        chmod +x "${BUILD_SERVER_PATH}/Server.x86_64"
    else
        echo "❌ Fichier Server.x86_64 non trouvé"
        exit 1
    fi
}

# Créer les dossiers de build
mkdir -p "${BUILD_CLIENT_PATH}"
mkdir -p "${BUILD_SERVER_PATH}"

# Exécuter les builds
if [ "$1" == "client" ]; then
    build_client
elif [ "$1" == "server" ]; then
    build_server
elif [ "$1" == "all" ] || [ -z "$1" ]; then
    echo "🔨 Build complet (Client + Serveur)"
    echo ""
    build_client
    echo ""
    build_server
    echo ""
    echo "✅ Tous les builds sont terminés"
else
    echo "Usage: $0 [client|server|all]"
    exit 1
fi
