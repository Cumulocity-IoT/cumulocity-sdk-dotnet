FROM mcr.microsoft.com/dotnet/sdk:8.0 as runtime


FROM runtime as release
RUN echo "Build release docker image..."

WORKDIR /app
COPY ./publish ./

ENTRYPOINT ["dotnet", "RestControllerExample.dll"]