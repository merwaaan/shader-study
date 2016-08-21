#version 440

layout(binding=0) uniform sampler2D diffuse_map;
layout(binding=1) uniform sampler2D specular_map;
layout(binding=2) uniform sampler2D normal_map;

uniform mat4 model_matrix;
uniform vec3 light_position;

in vec4 position;
in vec3 normal;
in vec3 tangent;
in vec3 bitangent;
in vec2 uv;

out vec4 frag_color;

const vec3 ambient_color = vec3(0.1, 0.1, 0.1);
const vec3 specular_color = vec3(1.0, 1.0, 1.0);
const float shininess = 32.0;

void main(void)
{
	mat3 tbn_matrix = mat3(
		tangent,
		bitangent,
		normal);

	vec3 normal = texture2D(normal_map, uv).xyz;
	normal = normalize(tbn_matrix * (normal * 2.0 - 1.0));
	
	vec3 diffuse_color = texture2D(diffuse_map, uv).rgb;

	// Lambertian component
	vec3 light_direction = normalize(light_position - position.xyz);
	float lambertian = max(dot(light_direction, normal), 0.0);

	// Specular component
	float specular = 0.0;

	if (lambertian > 0.0) {
	
		vec3 view_direction = normalize(-position).xyz;

		// Blinn
		//vec3 light_reflection = reflect(light_direction, normal);
		//specular = max(dot(light_reflection, view_direction), 0.0);
		//specular = pow(specular, shininess);// * ((shininess + 1) / (2 * 3.1415));

		// Blinn-Phong
		vec3 half_vector = normalize(light_direction + view_direction);
		specular = max(dot(half_vector, normal), 0.0);
		specular = pow(specular, shininess + texture2D(specular_map, uv).x * 100);
	}

	vec3 color = ambient_color + lambertian * diffuse_color + specular * specular_color;

	frag_color = vec4(color, 1.0);
}
