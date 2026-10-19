#!/bin/bash
# Runs on the GitHub runner``. Sends deploy/deploy.sh to an EC2 instance via SSM.
# usage: ssm-deploy.sh <Staging|Production> <image> <sha> <instance-id>
set -euo pipefail

ENVIRONMENT=${1:?usage: ssm-deploy.sh <Staging|Production> <image> <sha> <instance-id>}
IMAGE=${2:?missing image}
SHA=${3:?missing commit sha}
INSTANCE_ID=${4:?missing instance id}

export AWS_REGION=${AWS_REGION:-ap-southeast-2}
TIMEOUT=900   # seconds to wait for the deploy to finish
SCRIPT="$(dirname "$0")/deploy.sh"

# These values end up inside a shell command on the instance, so keep them boring
[[ "$ENVIRONMENT" =~ ^(Staging|Production)$ ]] || { echo "Environment must be Staging or Production"; exit 1; }
[[ "$IMAGE" =~ ^[A-Za-z0-9._/:@-]+$ ]]          || { echo "Unexpected characters in image: $IMAGE"; exit 1; }
[[ "$SHA" =~ ^[0-9a-f]{7,40}$ ]]                || { echo "Unexpected commit sha: $SHA"; exit 1; }
[[ "$INSTANCE_ID" =~ ^i-[0-9a-f]+$ ]]           || { echo "Unexpected instance id: $INSTANCE_ID"; exit 1; }
[ -f "$SCRIPT" ] || { echo "Can't find $SCRIPT"; exit 1; }

# Send deploy.sh as base64 so we don't have to escape it inside JSON
B64=$(base64 < "$SCRIPT" | tr -d '\n')
PARAMS=$(jq -n --arg b "$B64" --arg a "$ENVIRONMENT $IMAGE $SHA" \
  '{commands: ["echo \($b) | base64 -d > /tmp/deploy.sh", "bash /tmp/deploy.sh \($a)"]}')

CMD_ID=$(aws ssm send-command \
  --instance-ids "$INSTANCE_ID" \
  --document-name AWS-RunShellScript \
  --comment "deploy $SHA to $ENVIRONMENT" \
  --parameters "$PARAMS" \
  --query Command.CommandId --output text)
echo "Sent command $CMD_ID to $INSTANCE_ID ($ENVIRONMENT, $IMAGE)"

# Poll until the command finishes
START=$SECONDS
while :; do
  # right after sending, the invocation may not exist yet, so treat errors as Pending
  STATUS=$(aws ssm get-command-invocation --command-id "$CMD_ID" --instance-id "$INSTANCE_ID" \
    --query Status --output text 2>/dev/null || echo Pending)
  case "$STATUS" in
    Success) break ;;
    Pending|InProgress|Delayed) ;;
    *) break ;;   # Failed, TimedOut, Cancelled, ...
  esac
  if (( SECONDS - START > TIMEOUT )); then
    echo "Gave up waiting after ${TIMEOUT}s (last status: $STATUS)"; STATUS=TimedOut; break
  fi
  sleep 5
done

echo "----- output from the instance -----"
aws ssm get-command-invocation --command-id "$CMD_ID" --instance-id "$INSTANCE_ID" \
  --query StandardOutputContent --output text || true
if [ "$STATUS" != "Success" ]; then
  echo "----- errors -----"
  aws ssm get-command-invocation --command-id "$CMD_ID" --instance-id "$INSTANCE_ID" \
    --query StandardErrorContent --output text || true
  echo "Deploy finished with status: $STATUS"
  exit 1
fi
echo "Deploy succeeded"
