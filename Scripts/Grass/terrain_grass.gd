@tool
extends MultiMeshInstance3D

const MeshFactory = preload("res://Islands/Scripts/Grass/mesh_factory.gd")
const GrassFactory = preload("res://Islands/Scripts/Grass/grass_factory.gd")

@export var blade_width = Vector2(0.01, 0.02) : set = set_blade_width
@export var blade_height = Vector2(0.04, 0.08) : set = set_blade_height
@export var sway_yaw = Vector2(0.0, 10.0) : set = set_sway_yaw
@export var sway_pitch = Vector2(0.0, 10.0) : set = set_sway_pitch
@export var mesh:Mesh = null : set = set_mesh
@export var density:float = 1.0 : set = set_density

	
func _init():
	#rebuild()
	pass
	
func _ready():
	#rebuild()
	pass

func rebuild():
	if !mesh:
		return
	if !multimesh:
		multimesh = MultiMesh.new()
	multimesh.instance_count = 0
	var spawns:Array = GrassFactory.generate(
		mesh,
		density,
		blade_width,
		blade_height,
		sway_pitch,
		sway_yaw
	)
	if spawns.is_empty():
		return;
	multimesh.mesh = MeshFactory.simple_grass()
	multimesh.transform_format = MultiMesh.TRANSFORM_3D
	multimesh.use_custom_data = true
	multimesh.use_colors = false
	multimesh.instance_count = spawns.size()
	for index in (multimesh.instance_count):
		var spawn:Array = spawns[index]
		var basis = Basis(Vector3.UP, deg_to_rad(randf_range(0, 359)))
		multimesh.set_instance_transform(index, spawn[0])
		multimesh.set_instance_custom_data(index, spawn[1])
	
	
func set_blade_width(p_width):
	blade_width = p_width
	rebuild()

	
func set_blade_height(p_height):
	blade_height = p_height
	rebuild()

	
func set_sway_yaw(p_sway_yaw):
	sway_yaw = p_sway_yaw
	rebuild()

	
func set_sway_pitch(p_sway_pitch):
	sway_pitch = p_sway_pitch
	rebuild()

	
func set_mesh(p_mesh:Mesh):
	mesh = p_mesh
	rebuild()

	
func set_density(p_density:float):
	density = p_density
	if density < 1.0:
		density = 1.0
	rebuild()

