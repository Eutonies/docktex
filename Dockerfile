FROM ubuntu:latest as latexed
WORKDIR /tex
COPY proxy-root.crt /usr/local/share/ca-certificates/proxy-root.pem
COPY forti-proxy.crt /usr/local/share/ca-certificates/forti-proxy.pem
RUN apt-get update
RUN apt-get install -y ca-certificates
RUN update-ca-certificates
ENV http_proxy http://webproxy.nykreditnet.net:8080/
ENV https_proxy http://webproxy.nykreditnet.net:8080/
RUN apt-get install -y texlive-full

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["executor/Docktex.Executor/Docktex.Executor.csproj", "Docktex.Executor/"]
RUN apt-get install -y ca-certificates
COPY proxy-root.crt /usr/local/share/ca-certificates/proxy-root.pem
COPY forti-proxy.crt /usr/local/share/ca-certificates/forti-proxy.pem
COPY proxy-root.crt /etc/ssl/certs/proxy-root.pem
COPY forti-proxy.crt /etc/ssl/certs/forti-proxy.pem
RUN update-ca-certificates
RUN dotnet restore "Docktex.Executor/Docktex.Executor.csproj"
COPY executor/ Docktex.Executor/
WORKDIR "/src/Docktex.Executor"
RUN dotnet clean
RUN dotnet build "./Docktex.Executor.csproj" -c $BUILD_CONFIGURATION 
#-o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Docktex.Executor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false



FROM latexed as nextstage
COPY tex/fonts/ /usr/local/share/fonts
RUN fc-cache -fv
RUN apt-get update
RUN apt-get install -y ca-certificates
COPY proxy-root.crt /usr/local/share/ca-certificates/proxy-root.pem
COPY forti-proxy.crt /usr/local/share/ca-certificates/forti-proxy.pem
COPY proxy-root.crt /etc/ssl/certs/proxy-root.pem
COPY forti-proxy.crt /etc/ssl/certs/forti-proxy.pem

RUN update-ca-certificates
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
