#Stage 1: Build
FROM node:latest AS build

WORKDIR /app

# Installing the dependencies
COPY frontend/pnpm-lock.yaml frontend/package.json ./frontend/
COPY protobuf/ protobuf/
WORKDIR frontend
RUN npm i -g pnpm

RUN pnpm i

COPY . /app

# Building the application
RUN pnpm run build

#Stage 2: Production
FROM nginx:alpine AS production

COPY --from=build /app/frontend/dist /usr/share/nginx/html/
COPY frontend/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80     

CMD ["nginx","-g","daemon off;"]