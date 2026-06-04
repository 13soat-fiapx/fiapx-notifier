#!/bin/bash

QUEUES=(
  'fiapx-dev-video-status-changed'
)

for QUEUE in "${QUEUES[@]}"; do
  awslocal sqs create-queue --queue-name "$QUEUE" > /dev/null
  echo "Queue '$QUEUE' created."
done
