FROM golang:1.24 AS build
RUN apt-get update && apt-get install -y --no-install-recommends \
    libleptonica-dev libtesseract-dev libmupdf-dev && \
    rm -rf /var/lib/apt/lists/*
WORKDIR /src
COPY go.mod go.sum ./
RUN go mod download
COPY . .
RUN go build -o /app/server .

FROM debian:trixie-slim AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends \
    tesseract-ocr tesseract-ocr-por \
    libtesseract-dev libleptonica-dev \
    libmupdf-dev \
    ca-certificates && \
    rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/server .
ENTRYPOINT ["./server"]
