#
# NodeJS Docker image for LiveDataHandler.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

# Build stage
FROM node:alpine AS builder

WORKDIR /usr/src/app

COPY ./LiveDataHandler/package*.json ./
COPY ./LiveDataHandler/tsconfig*.json ./
COPY ./LiveDataHandler/src ./src
COPY ./LiveDataHandler/docker-settings ./settings
RUN npm ci --quit &&  npm run build

# Production stage
FROM node:alpine
WORKDIR /app
ENV NODE_ENV=production
ENV APP_PORT=5000
COPY ./LiveDataHandler/package*.json ./
RUN npm ci --quit --only=production

COPY --from=builder /usr/src/app/dist/ ./dist/

EXPOSE 5000
CMD ["node", "dist/src/app/live-data-handler.js"]
