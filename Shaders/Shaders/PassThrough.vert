#version 400

uniform mat4 mvp_matrix;

in vec3 vertex_position;

void main () {
    gl_Position = mvp_matrix * vec4(vertex_position, 1.0);
}
