#!/bin/bash
set -e

# Создаёт базы из переменной POSTGRES_MULTIPLE_DATABASES (через запятую)
# Пример: POSTGRES_MULTIPLE_DATABASES=myapp_identity,myapp_game

if [ -n "$POSTGRES_MULTIPLE_DATABASES" ]; then
    echo "Creating databases: $POSTGRES_MULTIPLE_DATABASES"
    for db in $(echo "$POSTGRES_MULTIPLE_DATABASES" | tr ',' ' '); do
        psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
            SELECT 'CREATE DATABASE $db'
            WHERE NOT EXISTS (
                SELECT FROM pg_database WHERE datname = '$db'
            )\gexec
            GRANT ALL PRIVILEGES ON DATABASE $db TO $POSTGRES_USER;
EOSQL
        echo "Database '$db' ready."
    done
fi