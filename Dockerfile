FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MultiPortUpload.sln ./
COPY src/MultiPortUpload.Api/MultiPortUpload.Api.csproj src/MultiPortUpload.Api/
COPY src/MultiPortUpload.Application/MultiPortUpload.Application.csproj src/MultiPortUpload.Application/
COPY src/MultiPortUpload.Infrastructure/MultiPortUpload.Infrastructure.csproj src/MultiPortUpload.Infrastructure/
COPY src/MultiPortUpload.Domain/MultiPortUpload.Domain.csproj src/MultiPortUpload.Domain/

RUN dotnet restore src/MultiPortUpload.Api/MultiPortUpload.Api.csproj

COPY . .
RUN dotnet publish src/MultiPortUpload.Api/MultiPortUpload.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends iproute2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MultiPortUpload.Api.dll"]