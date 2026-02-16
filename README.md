# Where We Go

## Prerequisites

- .NET 10.0 SDK
- Docker and Docker Compose


## Quick Start (How to run)

```
docker-compose up -d
dotnet ef database update
dotnet watch run
```

## Docker Compose Commands

### 1. Start All Services (Run Database)
```bash
docker-compose up -d
```


### 4. Stop All Services
```bash
docker-compose down
```

### 5. Stop and Remove Volumes (Delete Database Data)
```bash
docker-compose down -v
```

### 6. Restart Services
```bash
docker-compose restart
```

### 7. Check Service Status
```bash
docker-compose ps
```

### 8. Access Database Container
```bash
docker exec -it where_we_go_db psql -U postgres -d where_we_go_db
```

## Command for Project

### 1. Restore Dependencies
```bash
dotnet restore
```

### 2. Build Project 
```bash
dotnet build
```

### 3. Run Project
```bash
dotnet run
```

### 4. Run in Watch Mode (Auto-reload)
```bash
dotnet watch run
```

### 5. Clean Build Files 
```bash
dotnet clean
```

### 6. Database Migrations
```bash
# Create new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## Running the Complete Project

1. Start Database with Docker Compose:
   ```bash
   docker-compose up -d
   ```

2. Wait for Database to be ready (approximately 5-10 seconds)

3. Update Database schema:
   ```bash
   dotnet ef database update
   ```

4. Run the application:
   ```bash
   dotnet run
   ```