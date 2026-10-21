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

# The SQLite file lives in /data so a Railway Volume mounted there survives redeploys.
# The keys that sign staff sessions and form tokens are kept there too. 700 so only the app
# account can read them: the data-protection keys decrypt the sign-in cookies and form tokens.
RUN mkdir -p /data/keys

# This image is meant to run behind Railway's proxy, which overwrites X-Forwarded-For with the
# real client address, so the forwarded headers are trusted. Where the container is reached
# directly - docker compose, a plain `docker run` - this is set to false (see docker-compose.yml),
# so the rate limiter keys on the un-spoofable connection address instead. See the ForwardedHeaders
# note in Program.cs for why trusting the header off a real proxy would let the limit be bypassed.
ENV TrustProxy="true"

ENV ConnectionStrings__DefaultConnection="Data Source=/data/philisa.db"
ENV DataProtection__KeysPath="/data/keys"

# Run as the base image's non-root account rather than root, so a code-execution bug in the app
# can't act as root inside the container. /data is handed to it because that is the one place the
# app writes (the SQLite file and the data-protection keys).
#
# DevOps: a Railway Volume mounted at /data may come up owned by root. If the app can't write its
# database after this change, set the volume's mount to be writable by UID 1654, or run an init
# that chowns it. Verified writable under docker compose's named volume.
RUN chown -R app:app /data && chmod 700 /data /data/keys
USER app

# 8080 is plain HTTP, which Railway's proxy terminates TLS in front of. 8443 is HTTPS served by
# Kestrel itself, which docker-setup.ps1 wires up with a local development certificate so the app
# can be run over HTTPS on a machine with no proxy.
EXPOSE 8080
EXPOSE 8443

# Railway sets $PORT at runtime and expects plain HTTP on it; there is no certificate there, so
# TLS is the proxy's job. When ASPNETCORE_HTTPS_PORTS is set instead (docker compose, below), the
# app is left to bind both ports from the HTTP_PORTS / HTTPS_PORTS variables and serve TLS itself.
CMD ["sh", "-c", "if [ -z \"$ASPNETCORE_HTTPS_PORTS\" ]; then export ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}; fi; exec dotnet PhilisaAbantuBethu.dll"]
