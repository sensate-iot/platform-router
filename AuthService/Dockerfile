#
# AMD64 Dockerfile
#

FROM ubuntu:bionic

ENV LD_LIBRARY_PATH=/auth-service/lib:${LD_LIBRARY_PATH}

RUN apt-get update && apt-get install -y wget zip unzip
RUN wget https://staging.sensateiot.com/wp-content/uploads/2020/04/auth-service.zip && \
    unzip auth-service.zip

WORKDIR /auth-service
ENTRYPOINT ["/auth-service/bin/auth-service"]
