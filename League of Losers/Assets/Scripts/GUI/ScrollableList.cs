using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Classe qui permet de créer le tableau pour rejoindre une partie 
/// </summary>
public class ScrollableList : MonoBehaviour
{
    public GameObject itemPrefab;
    public int itemCount = 10, columnCount = 1;

    void Start()
    {
        this.Refresh();
    }

    public void Refresh()
    {
        /// rafraichissement de la liste des parties
        foreach (Transform child in gameObject.transform)
        {
            GameObject.Destroy(child.gameObject);
        };

        itemCount = PhotonNetwork.GetRoomList().Length;

        RectTransform rowRectTransform = itemPrefab.GetComponent<RectTransform>();
        RectTransform containerRectTransform = gameObject.GetComponent<RectTransform>();

        ///calculate the width and height of each child item.
        float width = containerRectTransform.rect.width / columnCount;
        float ratio = width / rowRectTransform.rect.width;
        float height = rowRectTransform.rect.height;
        int rowCount = itemCount / columnCount;
        if(rowCount != 0)
        {
            if (itemCount % rowCount > 0)
                rowCount++;
        }
        

        ///adjust the height of the container so that it will just barely fit all its children
        float scrollHeight = height * rowCount;
        containerRectTransform.offsetMin = new Vector2(containerRectTransform.offsetMin.x, -scrollHeight / 2);
        containerRectTransform.offsetMax = new Vector2(containerRectTransform.offsetMax.x, scrollHeight / 2);

        int i = 0, j = 0;
        foreach (RoomInfo game in PhotonNetwork.GetRoomList())
        {
            ///this is used instead of a double for loop because itemCount may not fit perfectly into the rows/columns
            if (i % columnCount == 0)
                j++;

            ///create a new item, name it, and set the parent
            GameObject newItem = Instantiate(itemPrefab) as GameObject;
            newItem.name = gameObject.name + " item at (" + i + "," + j + ")";
            newItem.transform.parent = gameObject.transform;
            newItem.transform.localScale = new Vector3(1,1);

            ///informations de la partie
            Text gameName = newItem.transform.GetChild(0).gameObject.GetComponent<Text>();
            gameName.text = game.name;
            Text gameMode = newItem.transform.GetChild(1).gameObject.GetComponent<Text>();
            gameMode.text = game.customProperties["Mode"].ToString();
            Text gamePlayerCount = newItem.transform.GetChild(2).gameObject.GetComponent<Text>();
            gamePlayerCount.text = game.playerCount.ToString();
            Button gameJoinButton =  newItem.GetComponent<Button>();
            gameJoinButton.onClick.AddListener(delegate { ConnectGame(game); });
                

            ///move and size the new item
            RectTransform rectTransform = newItem.GetComponent<RectTransform>();

            float x = -containerRectTransform.rect.width / 2 + width * (i % columnCount);
            float y = containerRectTransform.rect.height / 2 - height * j;
            rectTransform.offsetMin = new Vector2(x, y);

            x = rectTransform.offsetMin.x + width;
            y = rectTransform.offsetMin.y + height;
            rectTransform.offsetMax = new Vector2(x, y);
            i++;
        }
    }


    void ConnectGame(RoomInfo game)
    {
        ///Connexion à la partie
        if (game.open)
            PhotonNetwork.JoinRoom(game.name);
    }
}
