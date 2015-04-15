using UnityEngine;
using System.Collections;

public class RoomData {
    private int _height;
    private int _width;
    private int _area;
    private IntVector2 _origin;
    private string _name;

    public int Height { get { return _height; } set { _height = value; } }
    public int Width { get { return _width; } set { _width = value; } }
    public int Area { get { return _area; } set { _area = value; } }
    public IntVector2 Origin { get { return _origin; } set { _origin = value; } }
    public string Name { get { return _name; } set { _name = value; } }

    public RoomData(int width, int height)
    {
        this._height = height;
        this._width = width;
        _area = this._height * this._width;
        _origin = new IntVector2(0, 0);
    }

    public RoomData(int width, int height, IntVector2 origin)
    {
        this._height = height;
        this._width = width;
        _area = this._height * this._width;
        _origin = origin;
    }

    public override string ToString()
    {
        return "RoomData. Width: " + _width + ", Height: " + _height + ", Area: " + _area;
    }

}
