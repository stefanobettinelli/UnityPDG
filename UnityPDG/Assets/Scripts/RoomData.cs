using UnityEngine;
using System.Collections;

public class RoomData {
    private int _height;
    private int _width;
    private int _area;
    private IntVector2 _origin;
    private IntVector2 _center;
    private string _name;

    public int Height { get { return _height; } set { _height = value; } }
    public int Width { get { return _width; } set { _width = value; } }
    public int Area { get { return _area; } set { _area = value; } }
    public IntVector2 Origin { get { return _origin; } set { _origin = value; } }
    public IntVector2 Center { get { return _center; } set { _center = value; } }
    public string Name { get { return _name; } set { _name = value; } }

    public RoomData(int width, int height)
    {
        _height = height;
        _width = width;
        _area = _height * _width;
        _origin = new IntVector2(0, 0);
        _center = new IntVector2((int)Mathf.Floor(_width/2),(int)Mathf.Floor(_height/2));
        _name = "NONAME";
    }

    public RoomData(int width, int height, IntVector2 origin)
    {
        _height = height;
        _width = width;
        _area = _height * _width;
        _origin = origin;
    }

    public override string ToString()
    {
        return "RoomData of room: " + _name + " Width: " + _width + ", Height: " + _height + ", Area: " + _area + ", Center (" + _center.x + "," + _center.z + ")";
    }

}
