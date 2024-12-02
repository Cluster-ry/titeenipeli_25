#!/bin/bash
set -euo pipefail

if [ $# -ne 2 ]; then
  echo "Usage: $0 <docker context> <docker stack>"
  exit 1
fi

# shellcheck disable=SC2046
export $(cat .env) > /dev/null 2>&1

docker context use "$1"

docker compose build

docker stack deploy "$2" --detach=false -c docker-compose.yml
