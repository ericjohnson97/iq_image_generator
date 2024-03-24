# Intelligent Quads Image Generator

Over the years I have noticed it difficult to simulate camera streams on simulated drones at geo-specific locations. In addition existing options for geo-specific image generation are tied to simulators. This project aims to only handle the image generation portion of drone simulation. Because of this, the project is extensible and usable with any existing PX4 or Ardupilot drone simulator. 

## Underlying Technology

This project leverages Cesium for unity to bring geo-specific 3d map tiles into Unity. I have currently elected to use Google Maps as my 3d map tile provider. The map tile provider may be configurable in the future. The project then leverages FFmpeg to create video streams from the Unity cameras. The location of aircraft are brought into Unity using a websocket connection to a mavlink2REST server. 

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
- [ ] load in custom aircraft meshes
- [ ] add ground clipping system to allow ground vehicles to move across the terrain
- [ ] support multiple map tile providers

