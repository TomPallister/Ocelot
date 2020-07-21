# this script build the ocelot docker file
docker build -t mijitt0m/ocelot-build -f Dockerfile.base .
echo $DOCKER_PASS | docker login -u $DOCKER_USER --password-stdin
docker tag mijitt0m/ocelot-build mijitt0m/ocelot-build:0.0.1
docker push mijitt0m/ocelot-build:latest
docker push mijitt0m/ocelot-build:0.0.1
