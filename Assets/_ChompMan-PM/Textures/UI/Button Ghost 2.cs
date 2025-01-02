using UnityEngine;
using UnityEngine.UI;

public class SelectButton2 : MonoBehaviour
{
    public Button button2;

    void Start()
    {
        DisableNavigation(button2);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            button2.Select();
        }
    }

    void DisableNavigation(Button button)
    {
        var navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
    }
}
