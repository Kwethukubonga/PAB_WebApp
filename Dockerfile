FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY PhilisaAbantuBethu/PhilisaAbantuBethu.csproj PhilisaAbantuBethu/
RUN dotnet restore PhilisaAbantuBethu/PhilisaAbantuBethu.csproj
COPY PhilisaAbantuBethu/ PhilisaAbantuBethu/
RUN dotnet publish PhilisaAbantuBethu/PhilisaAbantuBethu.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# The SQLite file lives in /data so a Railway Volume mounted there survives redeploys.
RUN mkdir -p /data
ENV ConnectionStrings__DefaultConnection="Data Source=/data/philisa.db"

# Railway assigns the port at runtime through $PORT; listen on all interfaces.
CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet PhilisaAbantuBethu.dll"]
