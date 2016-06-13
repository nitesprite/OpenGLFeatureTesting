#version 430 core
 
in vec3 position3D;
in vec2 texturecoordinates;
uniform mat4 modelview;
out vec2 texturecoord;
 
void main(void) {
  gl_Position = modelview * vec4(position3D.x, position3D.y, position3D.z, 1);
  texturecoord = texturecoordinates;
}
