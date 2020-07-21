Building
========

* The best way to build Ocelot is using the Dockerfile.build file which can be found in the docker folder in Ocelot root. Use the following command `docker build -f ./docker/Dockerfile.build .`.

* You'll can run the `./build.ps1` or `./build.sh` script depending on your OS. This will compile, run unit and acceptance tests and build the output packages locally. Output will got to the `./artifacts` directory.

* There is a Makefile to make it easier to call the various targers in `build.cake`. The scripts are called with .sh but can be easily changed to ps1 if you are using Windows.

* Alternatively you can build the project in VS2019 with the latest .NET Core SDK.