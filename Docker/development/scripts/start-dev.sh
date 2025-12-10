#!/bin/bash
# ==============================================================================
# VUTEQ Development Environment - Start Script
# ==============================================================================
# Author: Hassan
# Date: 2025-11-11
# Description: One-command startup script for VUTEQ development environment
# ==============================================================================

set -e

echo "======================================================================"
echo "VUTEQ Development Environment - Startup"
echo "======================================================================"
echo "Author: Hassan"
echo "Date: $(date)"
echo "======================================================================"
echo ""

# Change to docker directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
DOCKER_DIR="$(dirname "$SCRIPT_DIR")"
cd "$DOCKER_DIR"

echo "Working Directory: $DOCKER_DIR"
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "ERROR: Docker is not running!"
    echo "Please start Docker Desktop and try again."
    exit 1
fi

echo "Docker Status: Running"
echo ""

# Check if docker-compose.dev.yml exists
if [ ! -f "docker-compose.dev.yml" ]; then
    echo "ERROR: docker-compose.dev.yml not found!"
    echo "Expected location: $DOCKER_DIR/docker-compose.dev.yml"
    exit 1
fi

echo "Configuration: docker-compose.dev.yml found"
echo ""

# Check if init files exist
if [ ! -f "init-db.sql" ]; then
    echo "WARNING: init-db.sql not found!"
    echo "Database initialization may fail."
fi

if [ ! -f "entrypoint.sh" ]; then
    echo "WARNING: entrypoint.sh not found!"
    echo "Database initialization may fail."
fi

echo ""
echo "======================================================================"
echo "Starting Services..."
echo "======================================================================"
echo ""

# Start services
docker compose -f docker-compose.dev.yml up -d

echo ""
echo "======================================================================"
echo "Waiting for services to be healthy..."
echo "======================================================================"
echo ""

# Function to check service health
check_service_health() {
    local service=$1
    local max_wait=$2
    local wait_time=0

    echo "Checking $service..."

    while [ $wait_time -lt $max_wait ]; do
        if docker inspect vuteq-$service-dev 2>/dev/null | grep -q '"Status": "healthy"'; then
            echo "$service is healthy!"
            return 0
        fi

        sleep 2
        wait_time=$((wait_time + 2))
        echo -n "."
    done

    echo ""
    echo "WARNING: $service did not become healthy in ${max_wait}s"
    return 1
}

# Check SQL Server (critical)
if ! check_service_health "sqlserver" 60; then
    echo ""
    echo "SQL Server health check failed. Checking logs..."
    docker logs --tail 30 vuteq-sqlserver-dev
fi

echo ""

# Check API
if ! check_service_health "api" 60; then
    echo ""
    echo "API health check failed. Checking logs..."
    docker logs --tail 30 vuteq-api-dev
fi

echo ""

# Check Frontend
if ! check_service_health "frontend" 60; then
    echo ""
    echo "Frontend health check failed. Checking logs..."
    docker logs --tail 30 vuteq-frontend-dev
fi

echo ""
echo "======================================================================"
echo "Service Status"
echo "======================================================================"
echo ""

docker compose -f docker-compose.dev.yml ps

echo ""
echo "======================================================================"
echo "Database Initialization Check"
echo "======================================================================"
echo ""

# Wait a bit more for database initialization to complete
sleep 5

# Check if database was created
DB_EXISTS=$(docker exec vuteq-sqlserver-dev /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P VuteqDev2025! \
    -Q "SELECT COUNT(*) FROM sys.databases WHERE name = 'VUTEQ_Scanner'" \
    -h -1 -W 2>/dev/null | tr -d ' ' || echo "0")

if [ "$DB_EXISTS" = "1" ]; then
    echo "Database: VUTEQ_Scanner - CREATED"

    # Get table count
    TABLE_COUNT=$(docker exec vuteq-sqlserver-dev /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P VuteqDev2025! -d VUTEQ_Scanner \
        -Q "SELECT COUNT(*) FROM sys.tables" \
        -h -1 -W 2>/dev/null | tr -d ' ' || echo "0")

    echo "Tables: $TABLE_COUNT"

    # Check sample users
    USER_COUNT=$(docker exec vuteq-sqlserver-dev /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P VuteqDev2025! -d VUTEQ_Scanner \
        -Q "SELECT COUNT(*) FROM dbo.Users" \
        -h -1 -W 2>/dev/null | tr -d ' ' || echo "0")

    echo "Sample Users: $USER_COUNT"
else
    echo "WARNING: Database VUTEQ_Scanner not found!"
    echo "Check initialization logs: docker logs vuteq-sqlserver-dev"
fi

echo ""
echo "======================================================================"
echo "VUTEQ Development Environment - READY"
echo "======================================================================"
echo ""
echo "Application URLs:"
echo "  Frontend:  http://localhost:3000"
echo "  Backend:   http://localhost:5000"
echo "  Swagger:   http://localhost:5000/swagger"
echo "  Health:    http://localhost:5000/health"
echo ""
echo "Database Connection:"
echo "  Host:      localhost"
echo "  Port:      1433"
echo "  Database:  VUTEQ_Scanner"
echo "  User:      sa"
echo "  Password:  VuteqDev2025!"
echo ""
echo "Sample Credentials:"
echo "  Admin:     admin / admin123"
echo "  Operator:  operator / operator123"
echo ""
echo "Useful Commands:"
echo "  View logs:     docker logs -f vuteq-api-dev"
echo "  Stop all:      docker compose -f docker-compose.dev.yml down"
echo "  Reset data:    docker compose -f docker-compose.dev.yml down -v"
echo "  Restart:       docker compose -f docker-compose.dev.yml restart"
echo ""
echo "Documentation:"
echo "  Dev Guide:     README_DEV.md"
echo "  DB Setup:      DATABASE_SETUP.md"
echo ""
echo "======================================================================"
echo "Happy Coding!"
echo "======================================================================"
