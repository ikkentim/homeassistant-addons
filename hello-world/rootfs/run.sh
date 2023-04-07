#!/usr/bin/env bashio

bashio::log.info "read"

MESSAGE=$(bashio::config 'message')

bashio::log.info "read2"

bashio::log.info "msg ${MESSAGE}"