#version 430 core
 
in vec2 texturecoord;
out vec4 outputColor;
uniform sampler2DArray textureArray;
uniform int texurelayer;
 
void main(void) {
	outputColor = texture(textureArray, vec3(texturecoord.x, texturecoord.y, texurelayer) ); 
}
