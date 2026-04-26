using UnityEngine;
using Yarn.Unity;

// DialogueTrigger.cs
//
// ЧТО ДЕЛАЕТ:
//   1. Запускает нужный узел диалога Yarn Spinner по имени.
//   2. Сообщает GameManager о начале/конце диалога
//      (чтобы заблокировать клики по сцене пока идёт диалог).
//   3. Регистрирует команды для .yarn файлов через [YarnCommand].
//
// КОМАНДЫ ДЛЯ .YARN ФАЙЛОВ:
//   <<add_token Revolt>> → +1 жетон Бунт
//   <<add_token Obedience>> → +1 жетон Послушание
//   <<add_token Analysis>> → +1 жетон Анализ
//   <<drink_cocktail>> → выпить коктейль (+Obedience, повтор +Analysis)
//   <<refuse_cocktail>> → отказ от коктейля (+Revolt)
//   <<load_next_scene>> → перейти к следующей сцене
//   <<enable_object ИмяОбъекта>> → включить кликабельный объект
//   <<launch_roulette>> → запустить мини-игру автомат

public class DialogueTrigger : MonoBehaviour
{
    [Header("Yarn Spinner")]
    [Tooltip("Компонент DialogueRunner на этом же объекте.\nПеретащи его сюда из компонентов ниже.")]
    public DialogueRunner dialogueRunner;

    void Start()
    {
        if (dialogueRunner == null)
        {
            Debug.LogError("[DialogueTrigger] dialogueRunner не назначен в Inspector! " +
                           "Перетащи компонент DialogueRunner в поле dialogueRunner.");
            return;
        }

        // Подписываемся на событие завершения диалога
        dialogueRunner.onDialogueComplete.AddListener(OnDialogueFinished);
    }

    void OnDestroy()
    {
        // Отписываемся чтобы не было утечек
        if (dialogueRunner != null)
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueFinished);
    }

    // Запуск диалога
    public void StartDialogueNode(string nodeName)
    {
        if (dialogueRunner == null) return;

        // Не запускаем если диалог уже идёт
        if (dialogueRunner.IsDialogueRunning)
        {
            Debug.LogWarning($"[DialogueTrigger] Диалог уже идёт, запрос узла '{nodeName}' проигнорирован");
            return;
        }

        // Сообщаем GameManager — теперь состояние Dialogue, клики заблокированы
        GameManager.Instance.OnDialogueStart();

        dialogueRunner.StartDialogue(nodeName);
        Debug.Log($"[DialogueTrigger] Запущен диалог: {nodeName}");
    }

    private void OnDialogueFinished()
    {
        // Сообщаем GameManager — диалог закончен, клики снова работают
        GameManager.Instance.OnDialogueEnd();
        Debug.Log("[DialogueTrigger] Диалог завершён");
    }

    // YARN КОМАНДЫ

    // Добавить жетон прямо из диалога
    [YarnCommand("add_token")]
    public void YarnAddToken(string tokenName)
    {
        if (System.Enum.TryParse<TokenType>(tokenName, out TokenType tokenType))
        {
            GameManager.Instance.gameState.AddToken(tokenType);
        }
        else
        {
            Debug.LogError($"[DialogueTrigger] Неизвестный жетон: '{tokenName}'. " +
                           "Используй: Revolt, Obedience, Analysis");
        }
    }

    // Эвелин выпила коктейль
    [YarnCommand("drink_cocktail")]
    public void YarnDrinkCocktail()
    {
        GameManager.Instance.gameState.DrinkCocktail();
    }

    // Эвелин отказалась от коктейля
    [YarnCommand("refuse_cocktail")]
    public void YarnRefuseCocktail()
    {
        GameManager.Instance.gameState.RefuseCocktail();
    }

    // Перейти к следующей сцене
    [YarnCommand("load_next_scene")]
    public void YarnLoadNextScene()
    {
        GameManager.Instance.LoadNextScene();
    }

    // Включить кликабельный объект по его имени в иерархии
    [YarnCommand("enable_object")]
    public void YarnEnableObject(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
        {
            Debug.LogError($"[DialogueTrigger] Объект '{objectName}' не найден в сцене. " +
                           "Проверь имя объекта в Hierarchy.");
            return;
        }

        InteractableObject interactable = obj.GetComponent<InteractableObject>();
        if (interactable != null)
            interactable.Enable(true);
        else
            Debug.LogWarning($"[DialogueTrigger] Объект '{objectName}' найден, " +
                             "но у него нет компонента InteractableObject.");
    }

    // Запустить мини-игру "Автомат" (слот-машина)
    [YarnCommand("launch_roulette")]
    public void YarnLaunchRoulette()
    {
        GameManager.Instance.LoadMiniGame("Roulette", MiniGameType.Roulette);
    }
}