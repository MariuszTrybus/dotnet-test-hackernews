default: build test

build:
    dotnet build

test:
    dotnet test

run:
    dotnet run --project src/HackerNewsReader.Api/HackerNewsReader.Api.csproj

build-hackernews-reader:
    nix build .#hackernews-reader
    
nuget2nix:
    cd src/HackerNewsReader.Api && \
    dotnet restore --packages out && \
    nuget-to-nix out > deps.nix

ide:
    nohup rider . &> /dev/null &
