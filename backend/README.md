# MoonTic

## Docker
- build for mac
docker build -f Ticketer.Web/Dockerfile -t ticketer-web .

- build/push for linux
  docker buildx build \
  --platform linux/amd64 \
  -f Ticketer.Web/Dockerfile \
  -t 333600347204.dkr.ecr.eu-west-1.amazonaws.com/moontic/prototype:latest \
  --push .

docker run -d -p 8080:8080 --env-file .test.env --name ticketer-web ticketer-web

- Push to ECR
docker push 333600347204.dkr.ecr.eu-west-1.amazonaws.com/moontic/prototype:latest
