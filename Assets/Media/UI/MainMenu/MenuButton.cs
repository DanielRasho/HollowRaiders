using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverAnim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator animator;

    public void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetBool("IsHovered", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool("IsHovered", false);
    }
}