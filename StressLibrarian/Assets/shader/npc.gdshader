shader_type spatial;

render_mode unshaded, cull_front, depth_draw_always; 

uniform vec4 outline_color : source_color = vec4(2.0, 0.7, 0.0, 1.0);
uniform float outline_size = 0.04;
uniform float pulse_speed = 2.0;

void vertex() {
    float pulse = sin(TIME * pulse_speed) * 0.01;
    VERTEX += NORMAL * (outline_size + pulse);
}

void fragment() {
    ALBEDO = outline_color.rgb;
	ALPHA = 0.0;
}

void light() {
    // Write 1 to the stencil buffer, ignore depth failure
    // Crucial part for Stencil 4.5+
    stencil_mode write, compare_always, 1;
}