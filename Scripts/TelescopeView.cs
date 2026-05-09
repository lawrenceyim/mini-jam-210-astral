using System;
using Godot;

public partial class TelescopeView : Node2D {
	[Export]
	private Camera2D _camera;

	[Export]
	private Sprite2D[] _stars;

	private float _starsMaxScale = 1.5f;
	private float _starsMinScale = .5f;
	private float _starsScaleSpeed = .5f;
	private float _currentStarsScale = 1f;

	private Vector2 _cameraSpeed = new(200, 200);

	public override void _Process(double delta) {
		_MoveCamera(delta);
		(int zoomInput, int rotationInput) = _GetCameraInput();
		_Zoom(zoomInput, delta);
	}

	private void _MoveCamera(double delta) {
		_camera.Position += _GetMovementInput() * _cameraSpeed * (float)delta;
	}

	private void _Zoom(int zoomInput, double delta) {
		if (zoomInput == 0) {
			return;
		}

		_currentStarsScale += _starsScaleSpeed * zoomInput * (float)delta;
		_currentStarsScale = Math.Clamp(_currentStarsScale, _starsMinScale, _starsMaxScale);
		Vector2 scale = new(_currentStarsScale, _currentStarsScale);
		foreach (Sprite2D star in _stars) {
			star.Scale = scale;
		}
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

	private (int zoomInput, int rotationInput) _GetCameraInput() {
		int zoomInput = 0;
		int rotationInput = 0;

		if (Input.IsKeyPressed(Key.Up)) {
			zoomInput += 1;
		}

		if (Input.IsKeyPressed(Key.Down)) {
			zoomInput -= 1;
		}

		if (Input.IsKeyPressed(Key.Left)) {
			rotationInput -= 1;
		}

		if (Input.IsKeyPressed(Key.Right)) {
			rotationInput += 1;
		}

		return (
			zoomInput,
			rotationInput
		);
	}
}
