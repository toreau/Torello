#!/bin/sh
#
rm -f Torello.Infrastructure/Migrations/* \
    && rm -f Torello.Api/torello.db* \
    && dotnet ef migrations add InitialCreate --project Torello.Infrastructure/ --startup-project Torello.Api/ \
    && dotnet ef database update --project Torello.Api/
