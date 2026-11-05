FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY PhilisaAbantuBethu/PhilisaAbantuBethu.csproj PhilisaAbantuBethu/
RUN dotnet restore PhilisaAbantuBethu/PhilisaAbantuBethu.csproj
COPY PhilisaAbantuBethu/ PhilisaAbantuBethu/
# UseAppHost=false skips the native launcher exe. Nothing starts the app that way: the container
# runs `dotnet PhilisaAbantuBethu.dll`, so the exe would only be dead weight in the image.
RUN dotnet publish PhilisaAbantuBethu/PhilisaAbantuBethu.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# The SQLite file lives in /data so a volume mounted there survives redeploys. The hosted site
# overrides ConnectionStrings__DefaultConnection with its Postgres database instead.
# The keys that sign staff sessions and form tokens are kept there too. 700 so only the app
# account can read them: the data-protection keys decrypt the sign-in cookies and form tokens.
RUN mkdir -p /data/keys

# Forwarded headers (the visitor's address, and whether they came in over HTTPS) are only trusted
# when TrustProxy is set, and it is deliberately not set here. A container reached directly -
# docker compose, a plain `docker run` - would let anyone forge those headers and get round the
# rate limit on the forms. The host puts TrustProxy=true in its own settings, because only there
# is a real proxy in front. See the ForwardedHeaders note in Program.cs.

ENV ConnectionStrings__DefaultConnection="Data Source=/data/philisa.db"
ENV DataProtection__KeysPath="/data/keys"

# Run as the base image's non-root account rather than root, so a code-execution bug in the app
# can't act as root inside the container. /data is handed to it because that is the one place the
# app writes (the SQLite file and the data-protection keys).
#
# Verified writable under docker compose's named volume. A host that mounts its own volume at
# /data would need it writable by this account too.
RUN chown -R app:app /data && chmod 700 /data /data/keys
USER app

# 8080 is plain HTTP, which the host's proxy terminates TLS in front of. 8443 is HTTPS served by
# Kestrel itself, which docker-setup.ps1 wires up with a local development certificate so the app
# can be run over HTTPS on a machine with no proxy.
EXPOSE 8080
EXPOSE 8443

# The host sets $PORT at runtime and expects plain HTTP on it; there is no certificate there, so
# TLS is the proxy's job. When ASPNETCORE_HTTPS_PORTS is set instead (docker compose, below), the
# app is left to bind both ports from the HTTP_PORTS / HTTPS_PORTS variables and serve TLS itself.
CMD ["sh", "-c", "if [ -z \"$ASPNETCORE_HTTPS_PORTS\" ]; then export ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}; fi; exec dotnet PhilisaAbantuBethu.dll"]
