#!/usr/bin/with-contenv bashio

set -e

DO_AUTH_TOKEN="$(bashio::config 'auth_token')"
RECORD_DOMAIN="$(bashio::config 'domain')"
RECORD_NAME="$(bashio::config 'name')"
IP_SERVICE="$(bashio::config 'ip_service')"

# Find the record
bashio::log.info "Looking up the currect DNS record..."
read -a values <<< "$(./doctl --access-token "$DO_AUTH_TOKEN" compute domain records list "$RECORD_DOMAIN" --format Type,Name,ID,Data --no-header | grep "A " | grep "$RECORD_NAME")"

RECORD_ID=${values[2]}
RECORD_DATA=${values[3]}

while true; do
    NEW_DATA=$(curl --fail -s "$IP_SERVICE")

    if [ "$NEW_DATA" == "$RECORD_DATA" ]; then
        bashio::log.info "The DNS record is up-to-date."
    else
        bashio::log.info "The DNS record will be updated from $RECORD_DATA to $NEW_DATA."
        ./doctl --access-token "$DO_AUTH_TOKEN" compute domain records update "$RECORD_DOMAIN" --record-id "$RECORD_ID" --record-data "$NEW_DATA"
        RECORD_DATA=$NEW_DATA
    fi

    sleep 900 # check again in 15 minutes
done



