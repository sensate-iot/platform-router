#
# NodeJS Docker image for LiveDataService.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

# Build stage
FROM node:alpine AS builder

WORKDIR /usr/src/app

COPY SensateIoT.Platform.Network.Contracts ./Contracts
COPY SensateIoT.Platform.Network.LiveDataService/package*.json ./
COPY SensateIoT.Platform.Network.LiveDataService/tsconfig*.json ./
COPY SensateIoT.Platform.Network.LiveDataService/src ./src
COPY SensateIoT.Platform.Network.LiveDataService/docker-settings ./settings

RUN npm ci --quit && mkdir generated && npm run generate-js-docker && npm run generate-ts && npm run build

# Production stage
FROM node:alpine

WORKDIR /app
ENV NODE_ENV=production
ENV APP_PORT=5000
COPY SensateIoT.Platform.Network.LiveDataService/package*.json ./
RUN npm ci --quit --only=production

COPY --from=builder /usr/src/app/dist/ /app/dist/
COPY --from=builder /usr/src/app/generated/ /app/dist/generated/

EXPOSE 5000
CMD ["node", "dist/src/app/live-data-service.js"]
