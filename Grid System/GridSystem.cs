//Example for an Grid-System
//github.com/n0j0games

using UnityEngine;

public class GridSystem : MonoBehaviour
{
    Grid<GameObject> grid = new Grid<GameObject>();
    [SerializeField] int xSpace, ySpace, xOffset, yOffset; //Space between each GameObject inside the Grid

    //Setting gameobject based on its coordinates
    void setGameObject(int x, int y, GameObject prefab)
    {
        GameObject temp = Instantiate(prefab);
        grid.setNode(x, y, temp);
        temp.transform.parent = gameObject.transform;
        temp.transform.position = new Vector3(x*xSpace+xOffset,y*ySpace+yOffset,0);
    }

    // Returning GameObject placed at grid pos (else null)
    GameObject getGameObject(int x, int y)
    {
        return grid.getValue(x, y);
    }

}
