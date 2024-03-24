# Intelligent Quads Image Generator

Over the years I have noticed it difficult to simulate camera streams on simulated drones at geo-specific locations. In addition, existing options for geo-specific image generation are tied to simulators. This project aims to only handle the image generation portion of drone simulation. Because of this, the project is extensible, distributable and usable with any existing PX4 or Ardupilot drone simulator. 

## What is an Image Generator?

An image generator is a application that can perform rendering of perspectives in distributed simulations. This is a common approach in professional aerospace flight simulators. For example in a large projector flight simulator room, the resolution of the view may be too high for a single computer to render. In this case, image generators are deployed to multiple computers which are in charge of rendering portions of the view.

## Intelligent Quads Image Generator Architecture

![architecture](imgs/IQ-Image-Generator.png)

## Underlying Technology

This project leverages Cesium for unity to bring geo-specific 3d map tiles into Unity. I have currently elected to use Google Maps as my 3d map tile provider. The map tile provider may be configurable in the future. The project then leverages FFmpeg to create video streams from the Unity cameras. The location of aircraft are brought into Unity using a websocket connection to a mavlink2REST server. 

### Technology Justification

- **Game Engine**: I considered using Unreal Engine, or GoDot as an alternative to Unity. Unreal would have worked just as well, however I am more familiar with unity and feel like the environment is easier to use. I did look at goDot, which would have been good because it would have alieviated licensing concerns some organizations will likely have with Unity. However, Godot did not have a Ceisum plugin and I did not want to write one.

- **Aircraft State Data Interface**: [Mavlink2REST](https://github.com/mavlink/mavlink2rest) While I could have used mavlink directly over UDP or TCP, I elected to use MAVLink2REST, because it allows for mavlink over websockets, which will allow for TLS encryption and enable the mavlink data to sit behind a reverse proxy, such as in the [Intelligent Quads Cloud Simulator](https://www.intelligentquads.com)


## Project Goals

- [x] Use 3d Map tiles to be able to have 3d image generation from any where in the world
- [x] generate udp h264 video streams from unity cameras
- [ ] support multiple aircraft
- [ ] create configuration file system to be able to configure
    - [ ] number of cameras
    - [ ] camera settings
    - [ ] aircraft meshs 
    - [ ] surface deflections and prop rotations
- [ ] in game menus for configuring settings
- [ ] modelling of mavlink based gimbal control
- [ ] load in custom aircraft meshes
- [ ] add ground clipping system to allow ground vehicles to move across the terrain
- [ ] support multiple map tile providers
- [ ] Set time of day
- [ ] Set weather conditions
- [ ] Some amount of world customization


## FAQ

### Are you planning on making this image generator CIGI compliant?

I am not planning on adding [CIGI](https://en.wikipedia.org/wiki/Common_Image_Generator_Interface) support to this project, because I do not have an immediate need for it. However, I am open to pull requests that add CIGI support.
