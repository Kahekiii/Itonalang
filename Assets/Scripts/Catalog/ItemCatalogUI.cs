using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class CatalogItem
{
    public string itemName;
    public Sprite itemSprite;
    [TextArea] public string description;
    public Vector3 dimensions;
}

public class ItemCatalogUI : MonoBehaviour
{
    [Header("Catalog Setup")]
    public List<CatalogItem> items;
    public GameObject itemButtonPrefab;
    public Transform contentParent;

    [Header("Details Panel")]
    public GameObject detailsPanel;
    public Image detailsImage;
    public TMP_Text detailsNameText;
    public TMP_Text detailsDescriptionText;
    public TMP_Text detailsDimensionsText;

    void Start()
    {
        PopulateCatalog();
        HideDetailsPanel();
    }

    void PopulateCatalog()
    {
        foreach (CatalogItem item in items)
        {
            GameObject buttonObj = Instantiate(itemButtonPrefab, contentParent);

            Image itemImage = buttonObj.transform.Find("ItemImage").GetComponent<Image>();
            if (itemImage != null)
                itemImage.sprite = item.itemSprite;

            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => ShowItemDetails(item));
            }
        }
    }


    public void ShowItemDetails(CatalogItem item)
    {
        detailsImage.sprite = item.itemSprite;
        detailsNameText.text = item.itemName;
        detailsDescriptionText.text = item.description;
        detailsDimensionsText.text = $"Dimensions:\nx: {item.dimensions.x}in, y: {item.dimensions.y}in, z: {item.dimensions.z}in";

        detailsPanel.SetActive(true);
    }

    public void HideDetailsPanel()
    {
        detailsPanel.SetActive(false);
    }
}
