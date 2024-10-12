{
  inputs = {
    nixpkgs = {
      url = "github:nixos/nixpkgs/nixos-unstable";
    };

    flake-utils = {
      url = "github:numtide/flake-utils";
    };
  };

  outputs = {
    self,
    nixpkgs,
    flake-utils,
  } @ inputs:
    flake-utils.lib.eachDefaultSystem (system: let
      pkgs = import nixpkgs {inherit system;};

      hackernews-reader = pkgs.buildDotnetModule rec {
        pname = "hackernews-reader";
        version = "0.0.1";

        src = ./.;
        nugetDeps = ./src/HackerNewsReader.Api/deps.nix;
        projectFile = "src/HackerNewsReader.Api/HackerNewsReader.Api.csproj";
        dotnet-sdk = pkgs.dotnetCorePackages.dotnet_8.sdk;
        dotnet-runtime = pkgs.dotnet-aspnetcore_8;
      };
    in {
      formatter = pkgs.alejandra;

      packages = {
        inherit hackernews-reader;
      };
      apps.hackernews-reader = {
        type = "app";
        program = "${hackernews-reader}/bin/HackerNewsReader.Api";
      };
      devShells.default = pkgs.mkShell {
        hardeningDisable = ["fortify"];
        inputsFrom = [
          hackernews-reader
        ];
        packages = with pkgs; [
          just
          nuget-to-nix
        ];
      };
    });
}
