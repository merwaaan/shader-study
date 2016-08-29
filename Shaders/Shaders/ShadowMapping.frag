#version 440

layout(binding=1) uniform sampler2D diffuse_map;
layout(binding=11) uniform sampler2D shadow_map;

uniform float shadow_bias;

in vec4 position_lightspace;
in vec2 uv;

out vec4 frag_color;

float shadow_mapping()
{
	vec3 p = position_lightspace.xyz / position_lightspace.w;
	p = p * 0.5 + 0.5;

	float obstacle_distance = texture2D(shadow_map, p.xy).r;
	float fragment_distance = p.z;

	return obstacle_distance < fragment_distance - shadow_bias ? 0.0 : 1.0;
}

void main(void)
{
	float shadow = shadow_mapping();
	frag_color = vec4(shadow, shadow, shadow, 1);
}
