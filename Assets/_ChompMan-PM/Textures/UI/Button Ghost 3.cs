using UnityEngine;
using UnityEngine.UI;

public class SelectButton3 : MonoBehaviour
{
    public Button button3;

    void Start()
    {
        DisableNavigation(button3);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            button3.Select();
        }
    }

    void DisableNavigation(Button button)
    {
        var navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
    }
}