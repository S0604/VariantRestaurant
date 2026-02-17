using UnityEngine;

public class ToggleObjects : MonoBehaviour
{
    public GameObject objectA; // ???
    public GameObject objectB; // ???

    public void SwitchObjects()
    {
        if (objectA != null)
            objectA.SetActive(true);

        if (objectB != null)
            objectB.SetActive(false);
    }
}
