#!/bin/bash
set -euo pipefail

ENVIRONMENT=${1:?usage: deploy.sh <Staging|Production> <image> <sha>}
IMAGE=${2:?missing image}
SHA=${3:?missing commit sha}

APP=app
REGION=ap-southeast-2
GHCR_USER=nhydnv
ENV_LC=${ENVIRONMENT,,}

case "$ENVIRONMENT" in
  Staging|Production) ;;
  *) echo "ENVIRONMENT must be Staging or Production"; exit 1 ;;
esac

# Refuse to deploy anything that isn't pinned to the commit SHA (e.g. :latest)
if [[ "$IMAGE" != *":$SHA" ]]; then
  echo "Image must be tagged with the commit SHA ($SHA), got: $IMAGE"; exit 1
fi

aws ssm get-parameter --region $REGION --with-decryption \
  --name "/$APP/$ENV_LC/GhcrToken" \
  --query Parameter.Value --output text \
  | docker login ghcr.io --username "$GHCR_USER" --password-stdin
docker pull "$IMAGE"

CONN=$(aws ssm get-parameter --region $REGION --with-decryption \
  --name "/$APP/$ENV_LC/ConnectionStrings__Default" \
  --query Parameter.Value --output text)

# Remember what's running now so we can roll back to it
PREV=$(docker inspect --format '{{.Config.Image}}' $APP 2>/dev/null || true)
PREV_SHA=${PREV##*:}

run() {  # $1=image $2=sha
  docker rm -f $APP 2>/dev/null || true
  docker run -d --name $APP --restart unless-stopped -p 80:8080 \
    -e ASPNETCORE_ENVIRONMENT="$ENVIRONMENT" \
    -e COMMIT_SHA="$2" \
    -e "ConnectionStrings__Default=$CONN" \
    --log-driver awslogs \
    --log-opt awslogs-region=$REGION \
    --log-opt awslogs-group="/$APP/$ENV_LC" \
    --log-opt awslogs-create-group=true \
    "$1"
}

healthy() {
  # up to ~80s: the app runs its EF migrations on startup, so the first checks can 503
  for _ in $(seq 1 40); do
    curl -fs http://localhost/health >/dev/null && return 0
    sleep 2
  done
  return 1
}

run "$IMAGE" "$SHA"
if healthy; then
  echo "Deployed $SHA to $ENVIRONMENT"
  curl -s http://localhost/health; echo
  docker image prune -af --filter "until=168h" >/dev/null || true
else
  echo "Health check failed for $SHA"; docker logs --tail 50 $APP || true
  if [ -n "$PREV" ]; then
    echo "Rolling back to $PREV"
    run "$PREV" "$PREV_SHA"
    healthy && echo "Rollback healthy" || echo "Rollback is ALSO unhealthy"
  fi
  exit 1
fi