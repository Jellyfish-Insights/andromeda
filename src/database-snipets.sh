# The following are some snippets to easy handling delopment database.

# Apply migrations
cd ConsoleApp && ./migrate.sh

# Create application migration
cd ApplicationModels
MIGRATION_UID=Migration
dotnet ef migrations add $MIGRATION_UID -s ../ConsoleApp

# Create data-lake migration
cd DataLakeModels
MIGRATION_UID=Migration
dotnet ef migrations add $MIGRATION_UID -s ../ConsoleApp -c DataLakeLoggingContext
dotnet ef migrations add $MIGRATION_UID -s ../ConsoleApp -c DataLakeYouTubeDataContext
dotnet ef migrations add $MIGRATION_UID -s ../ConsoleApp -c DataLakeYouTubeAnalyticsContext
dotnet ef migrations add $MIGRATION_UID -s ../ConsoleApp -c DataLakeAdWordsContext

# Drop the database in the container.
docker exec src_data_lake_1 dropdb data_lake -U fee -p 5433
docker exec src_analytics_platform_1 dropdb analytics_platform -U fee -p 5432

# Delete the latest migrations
cd ApplicationModels
dotnet ef migrations remove -s ../ConsoleApp
