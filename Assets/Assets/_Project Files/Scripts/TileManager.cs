using UnityEngine;
using UnityEngine.UI;

public class TileManager : MonoBehaviour
{
    public static TileManager instance;
    public Text Internal_Text;
    public Button Interactive_Button;

    private MultiplayerManager multiplayerManager;

    private void Awake()
    {
        instance = this;
        multiplayerManager = MultiplayerManager.instance;
        Interactive_Button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (!Interactive_Button.interactable)
            return;

        int index = transform.GetSiblingIndex();
        multiplayerManager.OnSetTurn(index);
    }
    public void ResetTile()
    {
        Internal_Text.text = ""; 
        Interactive_Button.image.sprite = GameManager.instance.Tile_Empty;
        Interactive_Button.interactable = true;
    }
}
