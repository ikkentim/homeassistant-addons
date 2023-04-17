#!/usr/bin/with-contenv bashio

bashio::log.info "Copying options..."

cp /data/options.json appsettings.json

dotnet SolisScraper.dll