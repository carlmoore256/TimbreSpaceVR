using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// public class UIListItem<T> {

// }


/// <summary>
/// A generic class that can make a scrollable list of items of type T 
/// </summary>
public class UIScrollableItemList<T> : MonoBehaviour {

    public GameObject itemPrefab;
    private List<T> items;
    // private List<SelectableListItem<T>> uiItems;
    public Transform content;
    public ScrollRect scrollRect;

    public void SetItems(List<T> items) {
        this.items = items;
        // this.uiItems = new List<SelectableListItem<T>>();
        foreach (T item in items) {
            GameObject itemUI = Instantiate(itemPrefab, content);
            // ScrollableItemUI<T> scrollableItemUI = itemUI.GetComponent<ScrollableItemUI<T>>();
            // scrollableItemUI.SetItem(item);
            // uiItems.Add(scrollableItemUI);
        }
    }


}



// public class ScrollableItemListUI<T> {

//     public GameObject itemPrefab;
//     public Transform content;
//     public ScrollRect scrollRect;

//     private List<T> items;
//     private List<ScrollableItemUI<T>> itemUIs;

//     public void SetItems(List<T> items) {
//         this.items = items;
//         this.itemUIs = new List<ScrollableItemUI<T>>();
//         foreach (T item in items) {
//             GameObject itemUI = Instantiate(itemPrefab, content);
//             ScrollableItemUI<T> scrollableItemUI = itemUI.GetComponent<ScrollableItemUI<T>>();
//             scrollableItemUI.SetItem(item);
//             itemUIs.Add(scrollableItemUI);
//         }
//     }

//     public void ScrollToItem(T item) {
//         int index = items.IndexOf(item);
//         if (index == -1) {
//             Debug.LogError("Item not found in list");
//             return;
//         }
//         float scrollPosition = (float)index / (items.Count - 1);
//         scrollRect.verticalNormalizedPosition = scrollPosition;
//     }

//     public void ScrollToItem(int index) {
//         if (index < 0 || index >= items.Count) {
//             Debug.LogError("Index out of range");
//             return;
//         }
//         float scrollPosition = (float)index / (items.Count - 1);
//         scrollRect.verticalNormalizedPosition = scrollPosition;
//     }

//     public void ScrollToItem(ScrollableItemUI<T> itemUI) {
//         int index = itemUIs.IndexOf(itemUI);
//         if (index == -1) {
//             Debug.LogError("Item not found in list");
//             return;
//         }
//         float scrollPosition = (float)index / (items.Count - 1);
//         scrollRect.verticalNormalizedPosition = scrollPosition;
//     }

//     public void ScrollToItem(ScrollableItemUI<T> itemUI, float scrollPosition) {
//         int index = itemUIs.IndexOf(itemUI);
//         if (index == -1) {
//             Debug.LogError("Item not found in list");
//             return;
//         }
//         scrollRect.verticalNormalizedPosition = scrollPosition;
//     }

//     public void ScrollToItem(int index, float scrollPosition) {
//         if (index < 0 || index >= items.Count) {
//             Debug.LogError("Index out of range");
//             return;
//         }
//         scrollRect.verticalNormalizedPosition = scrollPosition;
//     }

//     // public void ScrollToItem(T item, float scrollPosition) {
//     //     int index = items.IndexOf(item);
//     //     if (
// }