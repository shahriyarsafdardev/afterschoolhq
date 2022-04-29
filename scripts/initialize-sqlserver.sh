#!/bin/bash
docker compose -p afterschoolhq-standard -f ../../docker-compose.yml exec -T sql-server opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P testing123! -i scripts/setup/create-database.sql