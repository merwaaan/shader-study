#version 400

uniform mat4 MVPMatrix;

in vec3 vertex_position;
in vec3 vertex_color;

out vec4 color;

void main () {
    color = vec4(vertex_color, 1.0); // Just use the vertex color
    gl_Position = mvp_matrix * vec4(vertex_position, 1.0);
}
