// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

#version 460 core

// not wgsl because no preprocessor defines...

// relies on repacked vertex stream
layout(location = 0) in vec4 in_position;
layout(location = 1) in vec4 in_normal;
layout(location = 2) in vec4 in_uv; // also contains uv2.
#ifdef HAVE_TANGENT
layout(location = 3) in vec4 in_tangent;
layout(location = 4) in vec4 in_binormal;
#endif
#ifdef HAVE_BONE
layout(location = 5) in ivec4 in_bone_index;
layout(location = 6) in uint in_bone_weight;
#endif

layout(location = 0) out vec4 out_position;
layout(location = 1) out vec4 out_uv;
layout(location = 2) out vec3 out_normal;
#ifdef HAVE_TANGENT
layout(location = 3) out vec3 out_tangent;
layout(location = 4) out vec3 out_binormal;
#endif

layout(set = 0, binding = 1) uniform ModelMatrix {
	mat4 r_mmodel;
};
layout(set = 0, binding = 2) uniform ProjMatrix {
	mat4 r_mproj;
};
layout(set = 0, binding = 3) uniform ViewMatrix {
	mat4 r_mview;
};
layout(set = 0, binding = 4) uniform Time {
	float r_ftime;
};
#ifdef HAVE_BONE
layout(set = 2, binding = 1) uniform Bones {
	mat4 r_mbone[86]; // how many bones is too many bones? max is 255 / 3 = 85.
};
#endif

void main() {
	#ifdef HAVE_BONE
		vec4 position =
			(r_mbone[in_bone_index.x] * in_position) * in_bone_weight.x +
			(r_mbone[in_bone_index.y] * in_position) * in_bone_weight.y +
			(r_mbone[in_bone_index.z] * in_position) * in_bone_weight.z;
	#else
		vec4 position = in_position;
	#endif
	vec4 world_position = r_mmodel * position;

	gl_Position = r_mproj * r_mview * world_position;
	out_position = world_position;
	out_uv = in_uv;

	mat3 normal_matrix = transpose(inverse(mat3(r_mmodel)));
	out_normal = normalize(normal_matrix * in_normal.xyz);
	#ifdef HAVE_TANGENT
		out_tangent = normalize(normal_matrix * in_tangent.xyz);
		out_binormal = normalize(normal_matrix * in_binormal.xyz);
	#endif
}
