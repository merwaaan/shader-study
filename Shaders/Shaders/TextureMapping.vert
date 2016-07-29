#version 400

uniform mat4 mvp_matrix;

in vec3 vertex_position;
in vec2 vertex_texcoord;

out vec2 uv;

void main () {
    uv = vertex_texcoord;
    gl_Position = mvp_matrix * vec4(vertex_position, 1.0);
}
