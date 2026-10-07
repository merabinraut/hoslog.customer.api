FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS builder
WORKDIR /app
# Copy everything and build
COPY . .
RUN dotnet restore hoslog.customer.api/
RUN dotnet publish hoslog.customer.api/ -c Release -o release

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=builder /app/release .
COPY --from=builder /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

RUN apk add --no-cache icu-libs
#customuser
RUN addgroup --system --gid 1000 customgroup \
    && adduser --system --uid 1000 --ingroup customgroup --shell /bin/sh customuser

ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000

USER customuser

CMD ["dotnet", "hoslog.customer.api.dll"]
