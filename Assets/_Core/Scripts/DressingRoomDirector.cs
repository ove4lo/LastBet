using UnityEngine;

//   Управляет логикой сцены Scene2_Dressing
public class DressingRoomDirector : MonoBehaviour
{
    [Header("Кликабельные объекты сцены")]
    [Tooltip("Interactable_Note — записка от Виктора рядом с бокалом")]
    public InteractableObject noteInteractable;

    [Tooltip("Interactable_Cocktail — бокал с коктейлем.\n" +
             "Выключен при старте.\nВключается после прочтения записки.")]
    public InteractableObject cocktailInteractable;

    [Tooltip("Interactable_Door — дверь в бар")]
    public InteractableObject doorInteractable;

    [Tooltip("Interactable_Machine — старый автомат у двери.\n" +
             "Выключен при старте.\nВключается после попытки открыть дверь.")]
    public InteractableObject machineInteractable;

    [Header("Система диалогов")]
    [Tooltip("Компонент DialogueTrigger с объекта DialogueManager в этой сцене")]
    public DialogueTrigger dialogueTrigger;

    // Состояния сцены — не сохраняются (сцена проходится один раз)
    private bool _noteRead      = false;
    private bool _doorAttempted = false;

    void Start()
    {
        // Бокал скрыт до прочтения записки
        if (cocktailInteractable != null)
            cocktailInteractable.Enable(false);

        // Автомат скрыт до попытки открыть дверь
        if (machineInteractable != null)
            machineInteractable.Enable(false);
    }

    /// Клик на записку
    public void OnNoteClicked()
    {
        dialogueTrigger.StartDialogueNode("Dressing_Note");

        if (!_noteRead)
        {
            _noteRead = true;
            if (cocktailInteractable != null)
                cocktailInteractable.Enable(true);

            Debug.Log("[Dressing] Записка прочитана → бокал доступен");
        }
    }

    // Клик на бокал
    public void OnCocktailClicked()
    {
        dialogueTrigger.StartDialogueNode("Dressing_Cocktail");
    }

    // Клик на дверь
    public void OnDoorClicked()
    {
        if (!_doorAttempted)
        {
            _doorAttempted = true;

            if (machineInteractable != null)
                machineInteractable.Enable(true);

            Debug.Log("[Dressing] Дверь заперта → записка → автомат включён");
            dialogueTrigger.StartDialogueNode("Dressing_Door_Locked");
        }
        else
        {
            // Игрок снова кликнул на дверь — напомнить про автомат
            dialogueTrigger.StartDialogueNode("Dressing_Door_Remind");
        }
    }

    // Клик на автомат
    public void OnMachineClicked()
    {
        dialogueTrigger.StartDialogueNode("Dressing_Machine_Intro");
    }
}