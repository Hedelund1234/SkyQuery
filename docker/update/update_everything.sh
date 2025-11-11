# -e: stop hvis en kommando fejler
# -u: stop hvis u-defineret variabel bruges
# -o: Hvis du laver fx cmd1 | cmd2, og cmd1 fejler, så fejler hele kæden.
set -euo pipefail

echo "Opdaterer SkyQuery"

# Naviger til rod
cd ..
cd ..

# Git: opdater master
# Henter alle branches fra origin, uden at merge noget
git fetch --all --prune
# Skifter aktiv branch til master
git checkout master
# Henter nyeste ændringer fra origin/master og fast-forward-merger dem (ingen merge-commits)
git pull --ff-only origin master

# Docker Compose v2
# Stopper og fjerner alle containere defineret i docker-compose.prod.yml
docker compose -f docker-compose.prod.yml down --remove-orphans
# Forsøger at hente nyeste images fra registry (GHCR, Docker Hub osv.).
docker compose -f docker-compose.prod.yml pull || true
# Genstarter alt i baggrunden (-d) & Bygger images lokalt, hvis nødvendigt (--build).
docker compose -f docker-compose.prod.yml up -d --build

echo "SkyQuery er opdateret"
