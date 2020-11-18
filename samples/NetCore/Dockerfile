# escape=`

# BUILDER
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY src/ ./
RUN dotnet restore ConsoleApp/ConsoleApp.csproj
RUN dotnet publish ConsoleApp/ConsoleApp.csproj -c Release -o /out

# TOOLS
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS tools
ENV PATH="/root/.dotnet/tools:${PATH}"
RUN dotnet tool install -g EMG.Tools.EnsureUnique

# RUNNER
FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY --from=tools /root/.dotnet/tools/ /opt/bin
COPY --from=build /out .
ENV PATH="/opt/bin:${PATH}" `
    DotNetEnsureUnique__S3__BucketName= `
    DotNetEnsureUnique__S3__FilePrefix=
ENTRYPOINT dotnet-ensure-unique exe ./ConsoleApp
