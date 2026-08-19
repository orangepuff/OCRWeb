FROM node:22-alpine AS build
WORKDIR /app

# Install dependencies first (layer-cached separately from source)
COPY src/Frontend/OCRWeb.Frontend/package*.json ./
RUN npm ci

# Copy source and build production bundle
COPY src/Frontend/OCRWeb.Frontend/ ./
RUN npm run build

FROM nginx:alpine AS final
COPY --from=build /app/dist/OCRWeb.Frontend/browser /usr/share/nginx/html
COPY src/Frontend/OCRWeb.Frontend/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
