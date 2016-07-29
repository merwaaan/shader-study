#version 400

uniform mat4 mvp_matrix;

in vec3 vertex_position;

out vec4 color;

void main () {
    color = vec4(1.0, 0.0, 0.0, 1.0); // Everything is red
    gl_Position = mvp_matrix * vec4(vertex_position, 1.0);
}
