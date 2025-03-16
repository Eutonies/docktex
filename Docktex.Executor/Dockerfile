FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Docktex.Executor/Docktex.Executor.csproj", "Docktex.Executor/"]
RUN apt-get install -y ca-certificates
RUN update-ca-certificates
RUN dotnet restore "Docktex.Executor/Docktex.Executor.csproj"
COPY . .
WORKDIR "/src/Docktex.Executor"
RUN dotnet build "./Docktex.Executor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Docktex.Executor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM docker.io/suneroenne/docktex:latest AS nextstage
COPY tex/fonts/ /usr/local/share/fonts
RUN fc-cache -fv
RUN apt install -y software-properties-common
RUN apt install -y wget
RUN wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb
RUN apt update
RUN apt-get update
RUN add-apt-repository ppa:dotnet/backports
RUN apt-get install -y dotnet-sdk-9.0
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Docktex.Executor.dll"]
