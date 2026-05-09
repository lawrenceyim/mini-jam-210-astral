using Godot;

public partial class TelescopeView : Node2D {
	[Export]
	private Camera2D _camera;

	private Vector2 _cameraSpeed = new(200, 200);

	public override void _Process(double delta) {
		_camera.Position += _GetMovementInput() * _cameraSpeed * (float)delta;
	}

	private Vector2 _GetMovementInput() {
		Vector2 input = Vector2.Zero;

		if (Input.IsKeyPressed(Key.W)) {
			input.Y -= 1;
		}

		if (Input.IsKeyPressed(Key.S)) {
			input.Y += 1;
		}

		if (Input.IsKeyPressed(Key.A)) {
			input.X -= 1;
		}

		if (Input.IsKeyPressed(Key.D)) {
			input.X += 1;
		}

		return input.Normalized();
	}
}
