using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class InputTracker : MonoBehaviour
{
    public static InputTracker instance;
    public InputSystemUIInputModule inputSystemUIInputModule;
    public EventSystem eventSystem;
    public PlayerInput playerInput;
    public bool usingMouse = false;
    private string lastControlScheme;
    public Action OnInputChange;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public GameObject CurrentSelectedGameObject
    {
        get { return eventSystem.currentSelectedGameObject; }
    }

    public GameObject LastSelectedGameObject;

    public void SetLastSelectedGameObject(GameObject go)
    {
        LastSelectedGameObject = go;
        eventSystem.SetSelectedGameObject(go);
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if(playerInput == null)
        {
            playerInput = FindObjectOfType<PlayerInput>();
        }
        lastControlScheme = playerInput.currentControlScheme;
        if (lastControlScheme == "PC")
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            usingMouse = true;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            usingMouse = false;
            if (LastSelectedGameObject != null)
            {
                eventSystem.SetSelectedGameObject(LastSelectedGameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(playerInput == null)
        {
            playerInput = FindObjectOfType<PlayerInput>();
        }

        if(playerInput.currentControlScheme != lastControlScheme)
        {
            if(lastControlScheme == "PC")
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                usingMouse = false;
                eventSystem.SetSelectedGameObject(LastSelectedGameObject);
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                usingMouse = true;
            }
            lastControlScheme = playerInput.currentControlScheme;
            OnInputChange?.Invoke();
            print("Control Scheme Change, Invoke called");
        }
        if (usingMouse)
        {
            MouseSelection();
        }
        if (CurrentSelectedGameObject != LastSelectedGameObject)
        {
            if (CurrentSelectedGameObject == null)
            {
                return;
            }

            if (CurrentSelectedGameObject.gameObject.GetComponent<Button>() != null)
            {
                LastSelectedGameObject = CurrentSelectedGameObject;
            }

        }
    }

    void MouseSelection()
    {
        if (inputSystemUIInputModule.IsPointerOverGameObject(0))
        {
            var lasthit = inputSystemUIInputModule.GetLastRaycastResult(0);
            if(lasthit.gameObject !=null)
            {
                eventSystem.SetSelectedGameObject(lasthit.gameObject);
            }
        }
    }
}
