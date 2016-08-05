#version 440

uniform mat4 mvp_matrix;
uniform mat4 model_matrix;

in vec3 vertex_position;
in vec3 vertex_normal;
in vec3 vertex_tangent;
in vec3 vertex_bitangent;
in vec2 vertex_texcoord;

out vec4 position;
out vec2 uv;
out mat3 tbn_matrix;

out vec3 frag_position_tspace;
out vec3 view_position_tspace;

void main () {
	position = model_matrix * vec4(vertex_position, 1.0);

    vec3 normal = mat3(model_matrix) * vertex_normal;
    vec3 tangent = mat3(model_matrix) * vertex_tangent;
    vec3 bitangent = mat3(model_matrix) * vertex_bitangent;
	tbn_matrix = mat3(
		tangent,
		bitangent,
		normal);

    uv = vertex_texcoord;
	
	frag_position_tspace = tbn_matrix * position.xyz;
	view_position_tspace = tbn_matrix * vec3(0.0, 0.0, 1.0);

    gl_Position = mvp_matrix * vec4(vertex_position, 1.0);
}
