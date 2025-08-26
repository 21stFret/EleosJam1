using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CursorIcon : MonoBehaviour
{
    private Image cursorIcon;

    public Sprite defaultCursor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.GetComponent<Image>().sprite = defaultCursor;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Input.mousePosition;
    }
}
