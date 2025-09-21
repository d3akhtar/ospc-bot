#!/usr/bin/env bash

extract-card-number() {
  PR_TITLE="$1"
  CARD_NUMBER=$(echo "$PR_TITLE" | grep OSPC-[0-9]* -o | grep [0-9]* -o)
    
  if [[ -z "$CARD_NUMBER" ]]; then
    return 1
  else
    echo "$CARD_NUMBER"
    return 0
  fi
}

API_KEY="$TRELLO_API_KEY"
API_TOKEN="$TRELLO_API_TOKEN"

PR_TITLE="$1"

echo "Received PR title: $PR_TITLE"

CARD_NUMBER=$(extract-card-number "$PR_TITLE")

if [[ $? -eq 1 ]]; then
  echo -e "\033[0;31mPR Lint Check Failed: Couldn't find card number from PR title. It is preferred that the PR title follows this format: 'OSPC-(trello_card_number) ...'\033[1;37m"
  exit 1
fi

echo "Extracted card number: $CARD_NUMBER"

CARD_URL=$(curl --silent --url "https://api.trello.com/1/boards/jwQpp3ly/cards?key=$API_KEY&token=$API_TOKEN" | jq ".[] | select (.idShort == $CARD_NUMBER and (.name | startswith(\"OSPC-$CARD_NUMBER\"))).url")

if [[ -z "$CARD_URL" ]]; then
  echo -e "\033[0;31mPR Lint Check Failed: Couldn't find card with number $CARD_NUMBER\033[1;37m"
  exit 1
else
  echo -e "\033[0;32mPR Lint Check Passed\033[1;37m"
  echo -e "\033[1;35mCard URL Found: $CARD_URL\033[1;37m"
  exit 0
fi
