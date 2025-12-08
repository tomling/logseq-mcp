# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy source code (exclude solution file)
COPY Configuration/ ./Configuration/
COPY Models/ ./Models/
COPY Services/ ./Services/
COPY Tools/ ./Tools/
COPY Program.cs ./
COPY .editorconfig ./

# Build and publish
RUN dotnet publish -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Create non-root user for security
RUN groupadd -r mcpuser && useradd -r -g mcpuser mcpuser

# Copy published app from build stage
COPY --from=build /app ./

# Change ownership to non-root user
RUN chown -R mcpuser:mcpuser /app

# Switch to non-root user
USER mcpuser

# Set entry point
ENTRYPOINT ["dotnet", "LogseqMcpServer.dll"]
