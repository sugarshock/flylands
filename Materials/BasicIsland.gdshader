shader_type spatial;
render_mode blend_mix, diffuse_toon, specular_toon,ensure_correct_normals;

const float EPSILON = 0.09;

uniform vec3 dirt : source_color = vec3(1.0);
uniform vec3 grass : source_color = vec3(1.0);
uniform vec3 rock : source_color = vec3(1.0);
uniform vec3 sand : source_color = vec3(1.0);
uniform sampler3D terrain;

varying smooth vec3 our_color;

bool epsilon_comp(float v1, float v2){
	return(v1 >= v2 - EPSILON && v1 <= v2 + EPSILON);
}

vec3 terrain_to_color(float terrainType) {
	if(epsilon_comp(terrainType,0.0)){
		return vec3(1.0,0,0);
	}
	if(epsilon_comp(terrainType,0.1)){
		return dirt;
	}
	if(epsilon_comp(terrainType,0.2)){
		return grass;
	}
	if(epsilon_comp(terrainType,0.3)){
		return rock;
	}
	if(epsilon_comp(terrainType,0.4)){
		return sand;
	}
	return rock;
}

void vertex() {
	float bar_x = UV2.x;
	float bar_y = UV2.y;
	float bar_z = 1.0 - (bar_x + bar_y);
	
	vec3 color_x = terrain_to_color(COLOR.r);
	vec3 color_y = terrain_to_color(COLOR.g);
	vec3 color_z = terrain_to_color(COLOR.b);
	
	our_color = bar_x * color_x + bar_y * color_y + bar_z * color_z;
	ROUGHNESS = 1.0;
}



void fragment() {
	ALBEDO = our_color;
}
