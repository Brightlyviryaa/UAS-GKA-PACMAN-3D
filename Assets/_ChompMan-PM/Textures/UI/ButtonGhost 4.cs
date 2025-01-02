using UnityEngine;
using UnityEngine.UI;

public class SelectButton4 : MonoBehaviour
{
    public Button button4;

    void Start()
    {
        DisableNavigation(button4);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            button4.Select();
        }
    }

    void DisableNavigation(Button button)
    {
        var navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
    }
}
