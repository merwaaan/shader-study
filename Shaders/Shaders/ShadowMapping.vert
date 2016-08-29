#version 440

uniform mat4 model_matrix;
uniform mat4 mvp_matrix;
uniform mat4 light_matrix;

in vec3 vertex_position;
in vec2 vertex_texcoord;

out vec4 position_lightspace;
out vec2 uv;

void main () {
	vec4 position_worldspace = model_matrix * vec4(vertex_position, 1.0);
	position_lightspace = light_matrix * position_worldspace;

    uv = vertex_texcoord;

    gl_Position = mvp_matrix * vec4(vertex_position, 1.0);
}
