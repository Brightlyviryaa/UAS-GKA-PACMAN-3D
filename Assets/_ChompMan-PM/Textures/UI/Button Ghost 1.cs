using UnityEngine;
using UnityEngine.UI;

public class SelectButton1 : MonoBehaviour
{
    public Button button1;

    void Start()
    {
        DisableNavigation(button1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            button1.Select();
        }
    }

    void DisableNavigation(Button button)
    {
        var navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
    }
}