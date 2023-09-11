using UnityEngine;
using System.Collections;

/// <summary>
/// ToggleFace로 카드 스프라이트 바꿔줌
/// </summary>
public class CardModel : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    public Sprite[] faces;
    public Sprite cardBack;
    public int cardIndex;

    /// <summary>
    /// 참이면 앞면
    /// </summary>
    /// <param name="showFace"></param>
    public void ToggleFace(bool showFace)
    {
        if (showFace)
        {
            spriteRenderer.sprite = faces[cardIndex];
        }
        else
        {
            spriteRenderer.sprite = cardBack;
        }
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
