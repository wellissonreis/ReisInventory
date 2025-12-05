#!/usr/bin/env bash
set -e

DB_HOST=${DB_HOST:-db}
DB_PORT=${DB_PORT:-5432}
OLLAMA_HOST=${OLLAMA_HOST:-ollama}
OLLAMA_PORT=${OLLAMA_PORT:-11434}

echo "Aguardando PostgreSQL em ${DB_HOST}:${DB_PORT}…"
until nc -z "${DB_HOST}" "${DB_PORT}"; do
  echo "PostgreSQL não respondeu ainda. Tentando novamente em 2s…"
  sleep 2
done
echo "PostgreSQL OK."

echo "Aguardando Ollama em ${OLLAMA_HOST}:${OLLAMA_PORT}…"
until nc -z "${OLLAMA_HOST}" "${OLLAMA_PORT}"; do
  echo "Ollama não respondeu ainda. Tentando novamente em 2s…"
  sleep 2
done
echo "Ollama OK."

exec dotnet reis-inventory.dll
