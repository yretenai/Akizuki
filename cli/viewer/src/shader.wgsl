// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) color: vec4<f32>,
};

struct VertexInput {
    @location(0) position: vec4<f32>,
    @location(1) color: vec4<f32>,
};

@group(0) @binding(0)
var<uniform> r_vtime: f32;

@vertex
fn vs_main(vertex: VertexInput) -> VertexOutput {
    var result: VertexOutput;
    result.position = vertex.position;
    result.color = vertex.color;
    return result;
}

@group(0) @binding(1)
var<uniform> r_ftime: f32;

fn hsv2rgb(c: vec3<f32>) -> vec3<f32> {
    let k = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    let p = abs(fract(c.xxx + k.xyz) * 6.0 - k.www);
    return c.z * mix(k.xxx, clamp(p - k.xxx, vec3(0.0), vec3(1.0)), c.y);
}

@fragment
fn fs_main(vertex: VertexOutput) -> @location(0) vec4<f32> {
    let secs = (-r_ftime % 1.0) / 1.0;
    var hsv = vec3(0.0);
    hsv.x = secs;
    hsv.y = 1;
    hsv.z = 1;
    let rgb = vertex.color.rgb + hsv2rgb(hsv) / 2;
    return vec4(rgb, 1.0);
}
