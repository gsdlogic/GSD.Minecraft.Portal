# Command Reference

## Docker

Build image (run from solution directory):

```
docker build -f "Source\GSD.Minecraft.Portal\Dockerfile" --force-rm -t cr.example.com/gsd-minecraft-portal:latest --target final .
```

Push image to container registry:

```
docker login cr.examlple.com
docker push cr.example.com/gsd-minecraft-portal:latest
```

Start container from image:

```
docker run -d \
    --name GSD.Minecraft.Portal \
    -p 19132:19132/udp \
    -p \8080:8080 \
    -v C:\Users\Username\AppData\Local\GSD\MinecraftPortal:/usr/share/gsd/mcportal \
    cr.example.com/gsd-minecraft-portal:latest
```

## WSL

List installed distributions:

```
wsl -l
```

Install Ubuntu 22.04 distribution:

```
wsl --install -d Ubuntu-22.04
```

Uninstall Ubuntu 22.04 distribution:

```
wsl --unregister Ubuntu-22.04
```

Export distribution:

```
wsl --export Ubuntu-22.04 publish\wsl\ubuntu2204.tar
```

Import distribution:

```
wsl --import mcportal-test publish\wsl\mcportal-test publish\wsl\ubuntu2204.tar --version 2
```

Launch distribution:

```
wsl -d mcportal-test
```