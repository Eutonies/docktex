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
ENTRYPOINT ["tail", "-f", "/dev/null"]
