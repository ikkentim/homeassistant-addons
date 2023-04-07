ARG BUILD_FROM
FROM $BUILD_FROM

# Install requirements for add-on
RUN \
  apk add --no-cache \
    example_alpine_package

# Copy data for add-on
COPY rootfs /
RUN chmod a+x /run.sh

CMD [ "/run.sh" ]