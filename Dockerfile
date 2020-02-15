#======================================================================#
# This Dockerfile builds Andromeda docker image using                  #
# the REMOTE repository:                                               #
# https://github.com/Jellyfish-Insights/andromeda.git                  #
#======================================================================#
FROM mcr.microsoft.com/dotnet/core/sdk:2.1-bionic AS builder
WORKDIR /app

# You can specify these in docker-compose.yml or with
# docker build --build-args "ANDROMEDA_GIT_REF=git_branch_name" .
ARG ANDROMEDA_GIT_URL=https://github.com/Jellyfish-Insights/andromeda.git
ARG ANDROMEDA_GIT_REF=master

# Cloning from the remote repository and compiling the code
RUN git clone "$ANDROMEDA_GIT_URL" && \
  cd andromeda && \
  git checkout "$ANDROMEDA_GIT_REF" &&\
  cd Andromeda.ConsoleApp &&\
  dotnet publish -c release -o /app/release

# Multi-stage build
# Changing to the image that will run the project
FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-bionic
WORKDIR /app/release
# Copying all necessary files from the Builder Image
COPY --from=builder /app/release .
COPY --from=builder /app/andromeda/run.sh .
RUN chmod +x run.sh
# Create directory for facebook cache
RUN mkdir cache
CMD ./run.sh 2> /dev/null
