using System.Collections.Generic;
using Godot;

public class Constellations {
    // Made up grid values
    public static List<Vector2I> Aquarius = [
        new(2, 5),
        new(4, 6),
        new(6, 5),
        new(8, 6),
        new(10, 5),
        new(12, 6),
        new(13, 5),
        new(11, 10),
        new(7, 12)
    ];

    public static List<Vector2I> Aries = [
        new(2, 8),
        new(5, 6),
        new(8, 7),
        new(10, 10)
    ];

    public static List<Vector2I> Cancer = [
        new(2, 5),
        new(5, 3),
        new(8, 4),
        new(10, 7),
        new(8, 10),
        new(5, 11),
        new(3, 9)
    ];

    public static List<Vector2I> Capricornus = [
        new(2, 5),
        new(5, 3),
        new(8, 4),
        new(11, 7),
        new(10, 11),
        new(7, 13),
        new(3, 11)
    ];

    public static List<Vector2I> Gemini = [
        new(4, 1),
        new(10, 1),
        new(4, 4),
        new(10, 4),
        new(3, 7),
        new(5, 7),
        new(9, 7),
        new(11, 7),
        new(4, 11),
        new(10, 11)
    ];

    public static List<Vector2I> Leo = [
        new(10, 2),
        new(10, 4),
        new(8, 5),
        new(6, 7),
        new(4, 9),
        new(5, 11),
        new(8, 11),
        new(11, 11),
        new(13, 13)
    ];

    public static List<Vector2I> Libra = [
        new(2, 9),
        new(5, 6),
        new(7, 4),
        new(9, 6),
        new(12, 9),
        new(7, 10)
    ];

    public static List<Vector2I> Pisces = [
        new(2, 5),
        new(4, 3),
        new(6, 5),
        new(7, 8),
        new(8, 11),
        new(10, 13),
        new(9, 8),
        new(11, 5),
        new(13, 3)
    ];

    public static List<Vector2I> Sagittarius = [
        new(2, 10),
        new(5, 7),
        new(7, 4),
        new(9, 7),
        new(12, 10),
        new(7, 8),
        new(7, 13),
        new(10, 11)
    ];

    public static List<Vector2I> Scorpius = [
        new(2, 2),
        new(4, 4),
        new(6, 6),
        new(8, 7),
        new(10, 7),
        new(12, 9),
        new(12, 11),
        new(10, 13),
        new(8, 14)
    ];

    public static List<Vector2I> Taurus = [
        new(7, 1),
        new(4, 4),
        new(10, 4),
        new(2, 7),
        new(12, 7),
        new(7, 8),
        new(5, 11),
        new(9, 11)
    ];

    public static List<Vector2I> Virgo = [
        new(2, 2),
        new(4, 5),
        new(6, 8),
        new(8, 5),
        new(10, 2),
        new(11, 6),
        new(10, 10),
        new(8, 13)
    ];
}