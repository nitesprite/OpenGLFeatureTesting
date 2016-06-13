#version 430 core
 
in vec3 vposition;
in  vec3 vcolor;
out vec4 fcolor;
uniform mat4 vmodelview;
 
void main()
{
    gl_Position = vmodelview * vec4(vposition, 1.0);
 
    fcolor = vec4( vcolor, 1.0);
}
