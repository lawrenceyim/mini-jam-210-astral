using System.Collections.Generic;
using Godot;

public partial class TelescopeView : Node2D {
    [Export]
    private Camera2D _camera;

    [Export]
    private AudioStreamPlayer _sfxPlayer;

    [Export]
    private AudioStream _successSfx;

    [Export]
    private AudioStream _failedSfx;

    [Export]
    private Node2D _world;

    [Export]
    private Texture2D[] _starTextures;

    [Export]
    private Sprite2D _cameraUi;

    [Export]
    private Label _zoomLabel;

    [Export]
    private Label _scanStatusLabel;

    [Export]
    private PackedScene[] _starScenes;

    [Export]
    private PackedScene[] _galaxyScenes;

    [Export]
    private Texture2D _earthTexture;

    [Export]
    private Label _objectiveLabel;

    [Export]
    private Label _distanceLabel;

    private static readonly Vector2 _gridTileSize = new(64, 64);
    private static readonly int _gridSize = 500;

    // divide ui scale by zoom
    private float _maxCameraZoom = 2.25f;
    private float _minCameraZoom = .25f;
    private float _cameraZoomSpeed = 2f;

    private RandomNumberGenerator _rng = new();

    private Vector2 _cameraMoveSpeed = _gridTileSize * 15;

    private bool _scanKeyPressed = false;
    private float _scanTolerance = 10; // in tiles
    private double _scanCooldown = 1;
    private double _scanCooldownLeft = 0;

    private bool[,] _tileOccupied;
    private Vector2I _earthLocation;
    private List<Vector2I> _constellationStarPositions; // TODO: Init

    private int _numberOfStarTypes = 5;
    private int _numberOfGalaxyTypes = 6;

    private Vector2I _outOfBoundsBottomRightCorner = (Vector2I)(_gridTileSize * _gridSize);

    private Color _scannerReadyColor = new Color(0, 100, 0, 1);
    private Color _scannerRechargingColor = new Color(100, 0, 0, 1);

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
        _InitEarth();
        _InitConstellations();
        _InitRandomStars();
        _InitRandomGalaxies();
    }

    private void _InitGrid() {
        _tileOccupied = new bool[_gridSize, _gridSize];
    }

    private void _InitEarth() {
        int r = _rng.RandiRange(0, _gridSize);
        int c = _rng.RandiRange(0, _gridSize);
        _tileOccupied[r, c] = true;

        Sprite2D sprite = new();
        _world.AddChild(sprite);
        sprite.Texture = _earthTexture;
        sprite.Position = new Vector2(r, c) * _gridTileSize;
        _earthLocation = new Vector2I(r, c);
        GD.Print($"Earth location: {_earthLocation}");
    }

    private void _InitConstellations() {
        List<List<Vector2I>> constellations = [
            Constellations.Aquarius,
            Constellations.Aries,
            Constellations.Cancer,
            Constellations.Capricornus,
            Constellations.Gemini,
            Constellations.Leo,
            Constellations.Libra,
            Constellations.Pisces,
            Constellations.Sagittarius,
            Constellations.Scorpius,
            Constellations.Taurus,
            Constellations.Virgo
        ];

        List<Vector2I> constellationOffset = [
            new(-220, 220),
            new(-110, 220),
            new(0, 220),
            new(110, 220),

            new(-220, 110),
            new(-110, 110),
            new(0, 110),
            new(110, 110),

            new(-220, 0),
            new(-110, 0),
            new(0, 0),
            new(110, 0)
        ];

        Vector2I originTile = new(_gridSize / 2, _gridSize / 2);

        _constellationStarPositions = [];


        for (int i = 0; i < constellations.Count; i++) {
            foreach (Vector2I position in constellations[i]) {
                _constellationStarPositions.Add(position + originTile + constellationOffset[i]);
            }
        }

        foreach (Vector2I position in _constellationStarPositions) {
            _tileOccupied[position.X, position.Y] = true;
            // Sprite2D sprite = _CreateStarSprite(0); // Constellation stars are white
            AnimatedSprite2D sprite = _CreateAnimatedStarSprite(0);
            sprite.Position = position * _gridTileSize;
        }
    }

    private void _InitRandomStars() {
        for (int c = 0; c < _gridSize; c++) {
            for (int r = 0; r < _gridSize; r++) {
                if (_tileOccupied[c, r]) {
                    continue;
                }

                if (_rng.RandiRange(0, 100) < 99) {
                    continue;
                }

                _tileOccupied[c, r] = true;
                // Sprite2D sprite = _CreateStarSprite(_rng.RandiRange(1, _numberOfStarTypes));
                AnimatedSprite2D sprite = _CreateAnimatedStarSprite(_rng.RandiRange(1, _numberOfStarTypes - 1));
                sprite.Position = new Vector2I(c, r) * _gridTileSize;
            }
        }
    }

    private void _InitRandomGalaxies() {
        for (int c = 0; c < _gridSize; c++) {
            for (int r = 0; r < _gridSize; r++) {
                if (_tileOccupied[c, r]) {
                    continue;
                }

                if (_rng.RandiRange(0, 1000) < 990) {
                    continue;
                }

                _tileOccupied[c, r] = true;
                Sprite2D sprite = _CreateGalaxySprite(_rng.RandiRange(1, _numberOfGalaxyTypes - 1));
                sprite.Position = new Vector2I(c, r) * _gridTileSize;
            }
        }
    }

    private Sprite2D _CreateGalaxySprite(int textureId) {
        Sprite2D sprite = _galaxyScenes[textureId].Instantiate<Sprite2D>();
        _world.AddChild(sprite);
        return sprite;
    }

    private AnimatedSprite2D _CreateAnimatedStarSprite(int textureId) {
        AnimatedSprite2D sprite = _starScenes[textureId].Instantiate<AnimatedSprite2D>();
        _world.AddChild(sprite);
        return sprite;
    }

    private Sprite2D _CreateStarSprite(int textureId) {
        Sprite2D sprite = new();
        _world.AddChild(sprite);
        sprite.Texture = _starTextures[textureId];
        return sprite;
    }

    private void _MoveCamera(double delta) {
        Vector2 position = _camera.Position + _GetMovementInput() * _cameraMoveSpeed * (float)delta;
        _camera.Position = position.Clamp(Vector2.Zero, _outOfBoundsBottomRightCorner);
    }

    private void _Zoom(int zoomInput, double delta) {
        if (zoomInput == 0) {
            return;
        }

        _camera.Zoom += Vector2.One * _cameraZoomSpeed * (float)delta * zoomInput;
        _camera.Zoom = _camera.Zoom.Clamp(_minCameraZoom, _maxCameraZoom);

        _zoomLabel.Text = $"Zoom: {(_camera.Zoom.X + (1 - _minCameraZoom)):F2}×";
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

        _SetScanStatusText(false);
        Vector2I scannedTile = (Vector2I)(_camera.Position / _gridTileSize);
        GD.Print($"Scanning for {scannedTile} at Position {_camera.Position}");
        _scanCooldownLeft = _scanCooldown;
        _scanKeyPressed = true;

        GD.Print($"Scanned at {_camera.Position}");
        float distance = scannedTile.DistanceTo(_earthLocation);

        if (distance <= _scanTolerance) {
            GD.Print("Found earth");
            _sfxPlayer.Stream = _successSfx;
            _sfxPlayer.Play();
            _objectiveLabel.Text = "Earth found.";
        } else {
            _sfxPlayer.Stream = _failedSfx;
            _sfxPlayer.Play();
            _distanceLabel.Text = $"{(int)distance} Light Years";
        }
    }

    private void _ReduceCooldown(double delta) {
        _scanCooldownLeft -= delta;

        if (_scanCooldownLeft <= 0) {
            _SetScanStatusText(true);
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

    private void _SetScanStatusText(bool ready) {
        if (ready) {
            _scanStatusLabel.Text = "Ready";
            _scanStatusLabel.LabelSettings.FontColor = _scannerReadyColor;
            return;
        }

        _scanStatusLabel.Text = "Scanner Recharging";
        _scanStatusLabel.LabelSettings.FontColor = _scannerRechargingColor;
    }
}