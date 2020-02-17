#======================================================================#
# This Dockerfile builds Andromeda docker image using                  #
# your LOCAL copy of andromeda's directory. This image is useful when  #
# developing changing in the code.                                     #
#======================================================================#
FROM mcr.microsoft.com/dotnet/core/sdk:2.1-bionic AS builder
WORKDIR /app

# Copying all the files from the local root directory to the container
COPY . .

# Compiling the code
RUN cd Andromeda.ConsoleApp &&\
  dotnet publish -c release -o /app/release

# Multi-stage build
# Changing to the image that will run the project
FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-bionic
WORKDIR /app/release
# Copying all necessary files from the Builder Image
COPY --from=builder /app/release .
COPY --from=builder /app/run.sh .
RUN chmod +x run.sh
# Create directory for facebook cache
RUN mkdir cache
CMD ./run.sh
