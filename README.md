# Volumetric Clouds
* Made with Unity.
* A rendering experiment that creates 3D textures out of sprite sheets and samples the result texture to render a volume on top of everything else in a post-processing shader.
* The conversion from a 2D sprite sheet to a 3D texture is done using a compute shader that takes a sprite sheet and its metadata as input.
* The volume is rendered using raymarching and by sampling the 3D texture from the previous stage.
* The volume is rendered inside an AABB container.
