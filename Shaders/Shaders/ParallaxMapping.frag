#version 440

layout(binding=0) uniform sampler2D diffuse_map;
layout(binding=1) uniform sampler2D specular_map;
layout(binding=2) uniform sampler2D normal_map;
layout(binding=3) uniform sampler2D height_map;

uniform mat4 model_matrix;
uniform vec3 light_position;

in vec4 position;
in vec2 uv;
in mat3 tbn_matrix;

in vec3 frag_position_tspace;
in vec3 view_position_tspace;

out vec4 frag_color;

const vec3 ambient_color = vec3(0.01, 0.01, 0.01);
const vec3 specular_color = vec3(0.1, 0.1, 0.1);
const float shininess = 64.0;

vec2 ParallaxMapping(vec2 uv, vec3 view_direction)
{
	float height = texture2D(height_map, uv).r;
	vec2 offset = view_direction.xy * (1 - height) * 0.05;
	return uv + offset;
}

void main(void)
{
	// Offset texture coords for the parallax effect
	vec3 view_direction_tspace = normalize(view_position_tspace - frag_position_tspace);
    vec2 parallax = ParallaxMapping(uv, view_direction_tspace);

	vec3 normal = texture2D(normal_map, parallax).xyz;
	normal = normalize(tbn_matrix * (normal * 2.0 - 1.0));
	
	vec3 diffuse_color = texture2D(diffuse_map, parallax).rgb;

	// Lambertian component
	vec3 light_direction = normalize(light_position - position.xyz);
	float lambertian = max(dot(light_direction, normal), 0.0);

	// Specular component
	float specular = 0.0;

	if (lambertian > 0.0) {
	
		vec3 view_direction = normalize(-position).xyz;

		// Blinn-Phong
		vec3 half_vector = normalize(light_direction + view_direction);
		specular = max(dot(half_vector, normal), 0.0);
		specular = pow(specular, shininess + texture2D(specular_map, parallax).x);
	}

	vec3 color = ambient_color + lambertian * diffuse_color + specular * specular_color;

	frag_color = vec4(color, 1.0);
}
