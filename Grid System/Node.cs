//Node used by Grid
//github.com/n0j0games

public class Node<V>
{
    public Node<V> next;
    private int x;
    private int y;
    private V value;

    /// <summary>Initialize node</summary>
    public Node(int x, int y, V value)
    {
        this.x = x;
        this.y = y;
        this.value = value;
    }

    /// <summary>Set value of node</summary>
    public void setValue(V value)
    {
        this.value = value;
    }

    /// <summary>Set coordinates of node</summary>
    public void setCoordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    /// <summary>Returns value of node</summary>
    public V getValue()
    {
        return value;
    }

    /// <summary>Returns coordinates of node</summary>
    public (int,int) getCoordinates()
    {
        return (x,y);
    }


}
