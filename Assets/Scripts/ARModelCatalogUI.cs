using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ARModelCatalogUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform contentParent;

    [Header("Models to Load")]
    [SerializeField] private List<GameObject> modelPrefabs;
    [SerializeField] private List<Sprite> modelThumbnails;

    public static GameObject SelectedModel { get; private set; }

    public static ARModelCatalogUI Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        PopulateCatalog();
    }

    private void PopulateCatalog()
    {
        for (int i = 0; i < modelPrefabs.Count; i++)
        {
            GameObject model = modelPrefabs[i];
            GameObject button = Instantiate(buttonPrefab, contentParent);

            Image buttonImage = button.GetComponentInChildren<Image>();
            if (buttonImage != null && modelThumbnails.Count > i)
            {
                buttonImage.sprite = modelThumbnails[i];
            }

            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectModel(model);
            });
        }
    }

    private void SelectModel(GameObject model)
    {
        SelectedModel = model;
    }

    public static GameObject GetPrefabByName(string prefabName)
    {
        if (Instance == null)
        {
            return null;
        }

        foreach (var prefab in Instance.modelPrefabs)
        {
            if (prefab.name == prefabName)
            {
                return prefab;
            }
        }
        return null;
    }
}
