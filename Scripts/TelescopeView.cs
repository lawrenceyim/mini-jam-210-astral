using System;
using Godot;

public partial class TelescopeView : Node2D {
	[Export]
	private Camera2D _camera;

	[Export]
	private AudioStreamPlayer _sfxPlayer;

	[Export]
	private AudioStream _constellationFound1;

	[Export]
	private AudioStream _constellationFound2;

	[Export]
	private Node2D _world;

	[Export]
	private Texture2D[] _starTextures;

	[Export]
	private Sprite2D _cameraUi;

	[Export]
	private Label _zoomLabel;

	private static readonly Vector2 _gridTileSize = new(32, 32);

	// divide ui scale by zoom
	private float _maxCameraZoom = 2.5f;
	private float _minCameraZoom = .5f;
	private float _cameraZoomSpeed = 2f;

	private RandomNumberGenerator _rng = new();
	private Vector2I[] _constellationPositionTile;
	private bool[] _constellationFound;

	private Vector2 _cameraSpeed = _gridTileSize * 15;

	private bool _scanKeyPressed = false;
	private float _scanTolerance = 10; // in tiles which is 32 by 32
	private double _scanCooldown = 3;
	private double _scanCooldownLeft = 0;

	private bool[,] _hasStar;
	private int _gridSize = 500;
	private Vector2I[] _constellationStarPositions; // TODO: Init

	public override void _Ready() {
		_camera.Position = _gridSize / 2f * _gridTileSize;
		_InitWorld();
	}

	public override void _Process(double delta) {
		_MoveCamera(delta);
		(int zoomInput, int rotationInput) = _GetCameraInput();
		_Zoom(zoomInput, delta);
		_ReduceCooldown(delta);
		_ScanConstellation();
	}

	private void _InitWorld() {
		_InitGrid();
		_InitConstellations();
		_InitRandomStars();
	}

	private void _InitGrid() {
		_hasStar = new bool[_gridSize, _gridSize];
	}

	private void _InitConstellations() {
		_constellationStarPositions = [
			new Vector2I(0, 0), // 0
			new Vector2I(5, 0),
			new Vector2I(4, 2),
			new Vector2I(1, 7),
		]; // TODO: Init
		_constellationPositionTile = [
			new Vector2I(0, 0), // 0
		]; // TODO: Init
		_constellationFound = new bool[_constellationPositionTile.Length];
		foreach (Vector2I position in _constellationStarPositions) {
			_hasStar[position.X, position.Y] = true;
			Sprite2D sprite = _CreateStarSprite(0); // Constellation stars are white
			sprite.Position = position * _gridTileSize;
		}
	}

	private void _InitRandomStars() {
		for (int c = 0; c < _gridSize; c++) {
			for (int r = 0; r < _gridSize; r++) {
				if (_hasStar[c, r]) {
					continue;
				}

				if (_rng.RandiRange(0, 100) < 98) {
					continue;
				}

				Sprite2D sprite = _CreateStarSprite(_rng.RandiRange(1, 5)); // 5 non-white color options
				sprite.Position = new Vector2I(c, r) * _gridTileSize;
			}
		}
	}

	private Sprite2D _CreateStarSprite(int textureId) {
		Sprite2D sprite = new();
		_world.AddChild(sprite);
		sprite.Texture = _starTextures[textureId];
		return sprite;
	}

	private void _MoveCamera(double delta) {
		// TODO: Add out of bounds for the camera
		_camera.Position += _GetMovementInput() * _cameraSpeed * (float)delta;
	}

	private void _Zoom(int zoomInput, double delta) {
		if (zoomInput == 0) {
			return;
		}

		_camera.Zoom += Vector2.One * _cameraZoomSpeed * (float)delta * zoomInput;
		_camera.Zoom = _camera.Zoom.Clamp(_minCameraZoom, _maxCameraZoom);

		_zoomLabel.Text = $"Zoom: {(_camera.Zoom.X + .5f):F2}×";
	}

	private void _ScanConstellation() {
		bool keyPressed = Input.IsKeyPressed(Key.Space);

		if (!keyPressed) {
			_scanKeyPressed = false;
			return;
		}

		if (_scanKeyPressed) {
			return;
		}

		if (_scanCooldownLeft > 0) {
			GD.Print($"Scan is on cooldown for {_scanCooldownLeft} seconds");
			return;
		}

		Vector2I scannedTile = (Vector2I)(_camera.Position / _gridTileSize);
		GD.Print($"Scanning for {scannedTile} at Position {_camera.Position}");

		_scanCooldownLeft = _scanCooldown;

		_scanKeyPressed = true;
		GD.Print($"Scanned at {_camera.Position}");
		for (int i = 0; i < _constellationPositionTile.Length; i++) {
			if (_constellationFound[i]) {
				continue;
			}

			if (!(scannedTile.DistanceTo(_constellationPositionTile[i]) <= _scanTolerance)) {
				continue;
			}

			GD.Print($"Found constellation {i} at {_constellationPositionTile[i]}");
			_constellationFound[i] = true;
			_sfxPlayer.Stream = _rng.RandiRange(0, 1) == 0 ? _constellationFound1 : _constellationFound2;
			_sfxPlayer.Play();
			// Play VFX
			break;
		}
	}

	private void _ReduceCooldown(double delta) {
		_scanCooldownLeft -= delta;
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
