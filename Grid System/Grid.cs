//Grid used by Grid-Systems
//github.com/n0j0games

using UnityEngine;

public class Grid<V>
{
    Node<V> start;

    /// <summary>Creates new grid</summary>
    public Grid()
    {
        start = null;
    }

    /// <summary>Change node value if it's empty</summary>
    public void setIfEmpty(int x, int y, V value)
    {
        Node<V> current = getNode(x, y);
        if (current == null)
            setNode(x, y, value);
    }

    /// <summary>Set node value</summary>
    public void setNode(int x, int y, V value)
    {
        Node<V> current = getNode(x, y);
        if (current != null)
            current.setValue(value);
        else
        {
            Node<V> newNode = new Node<V>(x, y, value);
            newNode.next = start;
            start = newNode;
        }
    }

    /// <summary>Printing out grid in console for debug purpose</summary>
    public void printGrid()
    {
        Node<V> n = start;
        while (n != null)
        {
            Debug.Log("[" + n.getCoordinates().Item1 + "," + n.getCoordinates().Item2 + "]: " + n.getValue());
            n = n.next;
        }
    }

    /// <summary>Returns node value</summary>
    public V getValue(int x, int y)
    {
        Node<V> n = getNode(x, y);
        if (n == null)
            return default;
        return n.getValue();
    }

    /// <summary>Returns node at specific position</summary>
    public Node<V> getNode(int x, int y)
    {
        Node<V> n = start;
        while (n != null)
        {
            if (n.getCoordinates().Item1 == x && n.getCoordinates().Item2 == y)
                return n;
            n = n.next;
        }
        return null;
    }
}
