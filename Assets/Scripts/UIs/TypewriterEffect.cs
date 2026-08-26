using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TypewriterEffect : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float timePerCharacter = 0.05f;
    [SerializeField] private float punctuationDelay = 0.4f;

    private TextMeshProUGUI textMeshPro;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    // Fast-lookup for punctuation pauses
    private readonly HashSet<char> punctuationMarks = new HashSet<char> { '.', '!', '?', ',', ';', ':' };

    private void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        //ShowText("Press E to interact!");
        //ShowText("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam sed erat felis.");
    }

    public void ShowText(string fullText)
    {
        // Stop any text currently being typed out
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        textMeshPro.text = fullText;
        textMeshPro.maxVisibleCharacters = 0; // Hide everything initially

        typingCoroutine = StartCoroutine(TypeTextRoutine());
    }

    private IEnumerator TypeTextRoutine()
    {
        isTyping = true;

        textMeshPro.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMeshPro.textInfo;
        int totalCharacters = textInfo.characterCount;

        int currentVisibleCount = 0;

        while (currentVisibleCount <= totalCharacters)
        {
            textMeshPro.maxVisibleCharacters = currentVisibleCount;

            if (currentVisibleCount < totalCharacters)
            {
                char characterTyped = textInfo.characterInfo[currentVisibleCount].character;

                if (punctuationMarks.Contains(characterTyped))
                {
                    yield return new WaitForSeconds(punctuationDelay);
                }
                else
                {
                    yield return new WaitForSeconds(timePerCharacter);
                }
            }

            currentVisibleCount++;
        }

        isTyping = false;
        typingCoroutine = null;
    }

    public void SkipToEnd()
    {
        if (!isTyping) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        textMeshPro.maxVisibleCharacters = textMeshPro.textInfo.characterCount;
        isTyping = false;
    }

    public bool IsTyping => isTyping;
}
