using System;
using Godot;

public partial class TelescopeView : Node2D {
    [Export]
    private Camera2D _camera;

    [Export]
    private Sprite2D[] _stars;

    [Export]
    private Vector2[] _constellationPositions;

    [Export]
    private AudioStreamPlayer _sfxPlayer;

    [Export]
    private AudioStream _constellationFound1;

    [Export]
    private AudioStream _constellationFound2;

    private RandomNumberGenerator _rng = new();

    private bool[] _constellationFound;

    private float _starsMaxScale = 1.5f;
    private float _starsMinScale = .5f;
    private float _starsScaleSpeed = .5f;
    private float _currentStarsScale = 1f;

    private Vector2 _cameraSpeed = new(200, 200);

    private bool _scanKeyPressed = false;
    private float _scanTolerance = 100f;
    private double _scanCooldown = 3;
    private double _scanCooldownLeft = 0;

    public override void _Ready() {
        _InitConstellations();
    }

    public override void _Process(double delta) {
        _MoveCamera(delta);
        (int zoomInput, int rotationInput) = _GetCameraInput();
        _Zoom(zoomInput, delta);
        _ReduceCooldown(delta);
        _ScanConstellation();
    }

    private void _InitConstellations() {
        _constellationFound = new bool[_constellationPositions.Length];
    }

    private void _MoveCamera(double delta) {
        // TODO: Add out of bounds for the camera
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

        _scanCooldownLeft = _scanCooldown;

        // REMOVE TESETING CODE
        _sfxPlayer.Stream = _rng.RandiRange(0, 1) == 0 ? _constellationFound1 : _constellationFound2;
        _sfxPlayer.Play();
        //

        _scanKeyPressed = true;
        GD.Print($"Scanned at {_camera.Position}");
        for (int i = 0; i < _constellationPositions.Length; i++) {
            if (_constellationFound[i]) {
                continue;
            }

            if (!(_camera.Position.DistanceTo(_constellationPositions[i]) <= _scanTolerance)) {
                continue;
            }

            GD.Print($"Found constellation {i} at {_constellationPositions[i]}");
            _constellationFound[i] = true;
            _sfxPlayer.Stream = _rng.RandiRange(0, 1) == 0 ? _constellationFound1 : _constellationFound2;
            _sfxPlayer.Play();
            // Play SFX
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