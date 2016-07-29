#version 400

uniform sampler2D diffuse_map;

in vec2 uv;

out vec4 frag_color;

void main(void)
{
	frag_color = texture2D(diffuse_map, uv).rgba;
}
