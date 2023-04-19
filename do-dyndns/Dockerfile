ARG BUILD_FROM

FROM $BUILD_FROM

WORKDIR /app

RUN \
    wget https://github.com/digitalocean/doctl/releases/download/v1.93.1/doctl-1.93.1-linux-amd64.tar.gz && \
    tar xf doctl-1.93.1-linux-amd64.tar.gz

# Copy data for add-on
COPY rootfs /
RUN chmod a+x ./run.sh

CMD [ "./run.sh" ]