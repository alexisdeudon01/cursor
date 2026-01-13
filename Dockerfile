# Dockerfile pour Unity 6000.3.0f1
# Utilisé pour les builds automatiques Client et Serveur

FROM unityci/editor:6000.3.0f1-linux-il2cpp-1.0.0

# Informations sur l'image
LABEL maintainer="thebestclient-agent"
LABEL description="Unity 6000.3.0f1 pour builds automatiques"
LABEL unity.version="6000.3.0f1"

# Variables d'environnement Unity
ENV UNITY_VERSION=6000.3.0f1
ENV UNITY_LICENSE_FILE=/root/.unity3d/Unity_lic.ulf

# Installation des dépendances nécessaires
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    git \
    curl \
    ca-certificates \
    libgl1-mesa-glx \
    libglib2.0-0 \
    libxext6 \
    libxrender-dev \
    libgconf-2-4 \
    libxi6 \
    libxtst6 \
    libxrandr2 \
    libasound2 \
    libpangocairo-1.0-0 \
    libatk1.0-0 \
    libcairo-gobject2 \
    libgtk-3-0 \
    libgdk-pixbuf2.0-0 \
    && rm -rf /var/lib/apt/lists/*

# Configuration du workspace
WORKDIR /workspace

# Copie du projet (sera fait par GitHub Actions ou docker-compose)
# COPY . /workspace

# Scripts de build
COPY .github/scripts/build-unity.sh /usr/local/bin/build-unity.sh
RUN chmod +x /usr/local/bin/build-unity.sh

# Point d'entrée par défaut
ENTRYPOINT ["/bin/bash"]
