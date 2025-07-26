using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertivalJumpTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Trigger vertical jump
            collision.GetComponent<ExorsistController>().TriggerVerticalJump();
        }
    }
}
