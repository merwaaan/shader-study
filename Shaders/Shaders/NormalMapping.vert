#version 400

uniform mat4 mvp_matrix;
uniform mat4 model_matrix;

in vec3 vertex_position;
in vec3 vertex_normal;
in vec3 vertex_tangent;
in vec3 vertex_bitangent;
in vec2 vertex_texcoord;

out vec4 position;
out vec3 normal;
out vec3 tangent;
out vec3 bitangent;
out vec2 uv;

void main () {
	position = model_matrix * vec4(vertex_position, 1.0);
    normal = mat3(model_matrix) * vertex_normal;
    tangent = mat3(model_matrix) * vertex_tangent;
    bitangent = mat3(model_matrix) * vertex_bitangent;
    uv = vertex_texcoord;

    gl_Position = mvp_matrix * vec4(vertex_position, 1.0);
}
