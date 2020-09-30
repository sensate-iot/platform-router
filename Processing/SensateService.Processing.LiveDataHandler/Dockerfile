#
# NodeJS Docker image for LiveDataHandler.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

# Build stage
FROM node:alpine AS builder

WORKDIR /usr/src/app

COPY ./Processing/SensateService.Processing.LiveDataHandler/package*.json ./
COPY ./Processing/SensateService.Processing.LiveDataHandler/tsconfig*.json ./
COPY ./Processing/SensateService.Processing.LiveDataHandler/src ./src
COPY ./Processing/SensateService.Processing.LiveDataHandler/docker-settings ./settings
RUN npm ci --quit &&  npm run build

# Production stage
FROM node:alpine

WORKDIR /app
ENV NODE_ENV=production
ENV APP_PORT=5000
COPY ./Processing/SensateService.Processing.LiveDataHandler/package*.json ./
RUN npm ci --quit --only=production

COPY --from=builder /usr/src/app/dist/ /app/dist/

EXPOSE 5000
CMD ["node", "dist/app/live-data-handler.js"]
