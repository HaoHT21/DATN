using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
[System.Serializable]
public class CycleContent
{
    [Header("--- NHÂN VẬT 1 (BÊN TRÁI) ---")]
    public string nameCharacter1 = "Player";
    public Sprite avatarCharacter1;
    [TextArea(3, 5)] public string contentForPanel1;

    [Header("--- NHÂN VẬT 2 (BÊN PHẢI) ---")]
    public string nameCharacter2 = "NPC";
    public Sprite avatarCharacter2;
    [TextArea(3, 5)] public string contentForPanel2;
}
public class DialogueManager : MonoBehaviour
{
    // Singleton giúp các NPC gọi tới Manager này dễ dàng
    public static DialogueManager Instance { get; private set; }

    [Header("UI Components")]
    public GameObject dialoguePanelObject; // Thanh Panel lớn duy nhất
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Avatar Components")]
    public Image leftAvatarImage;
    public Image rightAvatarImage;

    private NPCInteraction currentNPC;
    private CycleContent[] fullScript;
    private int currentCycleIndex = 0;
    private bool isPanel1Active = true;
    private float typingSpeed = 0.05f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (dialoguePanelObject != null) dialoguePanelObject.SetActive(false);
        SetAvatarsActive(false);
    }

    // Hàm này sẽ được NPC gọi khi người chơi tương tác
    // Hàm này sẽ được NPC gọi khi người chơi tương tác
    public void StartCycling(CycleContent[] script, NPCInteraction npc, float speed)
    {
        // 1. NGĂN CHẶN XUNG ĐỘT: Nếu panel đang mở, không cho phép kích hoạt hội thoại mới
        if (dialoguePanelObject != null && dialoguePanelObject.activeSelf)
        {
            Debug.LogWarning($"<color=yellow>[DialogueManager]</color> Hội thoại đang diễn ra, bỏ qua yêu cầu từ NPC: {npc.name}");
            return;
        }

        if (script == null || script.Length == 0) return;

        // 2. THIẾT LẬP TRẠNG THÁI
        Time.timeScale = 0f; // Đóng băng game
        currentNPC = npc;    // Lưu lại ĐÚNG con NPC này để EndCycling gọi lại đúng nó
        fullScript = script;
        typingSpeed = speed;
        currentCycleIndex = 0;
        isPanel1Active = true;

        // 3. HIỂN THỊ UI
        if (dialoguePanelObject != null) dialoguePanelObject.SetActive(true);
        SetAvatarsActive(true);

        // 4. BẮT ĐẦU HIỂN THỊ
        Debug.Log($"<color=green>[DialogueManager]</color> Đang hội thoại với NPC: {currentNPC.name}");
        UpdateDialogueUI();
    }

    void Update()
    {
        if (currentNPC == null || fullScript == null || fullScript.Length == 0 || !dialoguePanelObject.activeSelf)
            return;

        // Bấm phím hoặc chuột để đi tiếp
        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetMouseButtonDown(0))
        {
            HandleCycleProgression();
        }
    }

    // Đổi thành Public để nút mũi tên đỏ trong Unity có thể click gọi hàm này
    public void HandleCycleProgression()
    {
        string expectedContent = isPanel1Active ? fullScript[currentCycleIndex].contentForPanel1 :
                                                  fullScript[currentCycleIndex].contentForPanel2;

        if (dialogueText.text != expectedContent)
        {
            StopAllCoroutines();
            dialogueText.text = expectedContent;
            return;
        }

        if (isPanel1Active)
        {
            isPanel1Active = false;
            UpdateDialogueUI();
        }
        else
        {
            currentCycleIndex++;
            if (currentCycleIndex < fullScript.Length)
            {
                isPanel1Active = true;
                UpdateDialogueUI();
            }
            else
            {
                EndCycling();
            }
        }
    }

    private void UpdateDialogueUI()
    {
        CycleContent currentLine = fullScript[currentCycleIndex];

        string speakerName = isPanel1Active ? currentLine.nameCharacter1 : currentLine.nameCharacter2;
        string speakerContent = isPanel1Active ? currentLine.contentForPanel1 : currentLine.contentForPanel2;

        nameText.text = speakerName;

        if (currentLine.avatarCharacter1 != null) leftAvatarImage.sprite = currentLine.avatarCharacter1;
        if (currentLine.avatarCharacter2 != null) rightAvatarImage.sprite = currentLine.avatarCharacter2;

        if (isPanel1Active)
        {
            // Nhân vật 1 (Trái) đang nói: Sáng rõ 100%
            if (leftAvatarImage != null) leftAvatarImage.color = Color.white;

            // Nhân vật 2 (Phải) im lặng: Mờ tịt xuống còn 15% độ sáng và độ trong suốt
            if (rightAvatarImage != null) rightAvatarImage.color = new Color(0.15f, 0.15f, 0.15f, 0.15f);

            // TÙY CHỌN: Nếu muốn nhân vật im lặng BIẾN MẤT HẲN luôn thì xóa dấu // ở dòng dưới:
            // if (rightAvatarImage != null) rightAvatarImage.gameObject.SetActive(false);
        }
        else
        {
            // Nhân vật 2 (Phải) đang nói: Sáng rõ 100%
            if (rightAvatarImage != null) rightAvatarImage.color = Color.white;

            // Nhân vật 1 (Trái) im lặng: Mờ tịt xuống còn 15% độ sáng và độ trong suốt
            if (leftAvatarImage != null) leftAvatarImage.color = new Color(0.15f, 0.15f, 0.15f, 0.15f);

            // TÙY CHỌN: Nếu muốn nhân vật im lặng BIẾN MẤT HẲN luôn thì xóa dấu // ở dòng dưới:
            // if (leftAvatarImage != null) leftAvatarImage.gameObject.SetActive(false);
        }

        StopAllCoroutines();
        StartCoroutine(TypeContent(speakerContent));
    }

    IEnumerator TypeContent(string content)
    {
        dialogueText.text = "";
        foreach (char c in content.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }

    void SetAvatarsActive(bool active)
    {
        if (leftAvatarImage != null) leftAvatarImage.gameObject.SetActive(active);
        if (rightAvatarImage != null) rightAvatarImage.gameObject.SetActive(active);
    }

    void EndCycling()
    {
        dialoguePanelObject.SetActive(false);
        SetAvatarsActive(false);
        Time.timeScale = 1f; // Khôi phục thời gian game

        if (currentNPC != null)
        {
            currentNPC.BeginCombat(); // NPC kích hoạt trạng thái chiến đấu
        }

        currentNPC = null;
        Debug.Log("Kết thúc chuỗi hội thoại.");
    }
}