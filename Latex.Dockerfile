FROM ubuntu:latest as latexed
WORKDIR /tex
RUN apt-get update
RUN apt-get install -y ca-certificates
RUN update-ca-certificates
RUN apt-get install -y texlive-full
ENTRYPOINT ["tail", "-f", "/dev/null"]
