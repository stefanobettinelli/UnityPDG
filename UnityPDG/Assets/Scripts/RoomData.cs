using UnityEngine;
using System.Collections;

public class RoomData {
    private int height;
    private int width;
    private int area;

    public int Height { get { return height; } set { height = value; } }
    public int Width { get { return width; } set { width = value; } }
    public int Area { get { return area; } set { area = value; } }

    public RoomData(int width, int height)
    {
        this.height = height;
        this.width = width;
        area = this.height * this.width;
    }

    public override string ToString()
    {
        return "width: " + width + ",height: " + height + ",area: " + area;
    }

}
