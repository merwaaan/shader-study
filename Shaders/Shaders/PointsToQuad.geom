#version 440

layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

out vec2 uv;

float size = 0.05;

void main()
{
    gl_Position = gl_in[0].gl_Position + vec4(size, -size, 0.0, 0.0);
	uv = vec2(1, 0);
    EmitVertex();

    gl_Position = gl_in[0].gl_Position + vec4(-size, -size, 0.0, 0.0);
	uv = vec2(0, 0);
    EmitVertex();

    gl_Position = gl_in[0].gl_Position + vec4(size, size, 0.0, 0.0);
	uv = vec2(1, 1);
    EmitVertex();

    gl_Position = gl_in[0].gl_Position + vec4(-size, size, 0.0, 0.0);
	uv = vec2(0, 1);
    EmitVertex();

    EndPrimitive();
}
