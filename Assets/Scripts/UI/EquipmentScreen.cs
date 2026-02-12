using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 장비 업그레이드 화면
/// 4종 장비 (카메라, PC, 마이크, 조명) 관리
/// </summary>
public class EquipmentScreen : MonoBehaviour
{
    [Header("장비 슬롯")]
    public GameObject cameraSlot;
    public GameObject pcSlot;
    public GameObject microphoneSlot;
    public GameObject lightSlot;

    [Header("재화 표시")]
    public TextMeshProUGUI goldText;

    [Header("공통")]
    public TextMeshProUGUI statusText;
    public Button refreshButton;
    public Button backButton;

    private EquipmentData[] allEquipment;
    private long currentGold = 0;

    void Start()
    {
        refreshButton.onClick.AddListener(LoadEquipment);
        backButton.onClick.AddListener(OnBack);

        LoadEquipment();
    }

    void LoadEquipment()
    {
        StartCoroutine(APIClient.Instance.GetEquipment((success, response) =>
        {
            if (success)
            {
                var data = JsonUtility.FromJson<EquipmentListResponse>(response);
                allEquipment = data.Equipment;
                currentGold = data.Gold;

                UpdateUI();
                SetStatus($"장비 정보 갱신 완료");
            }
            else
            {
                SetStatus("장비 정보를 불러올 수 없습니다.");
            }
        }));
    }

    void UpdateUI()
    {
        goldText.text = $"골드: {currentGold:#,0}";

        if (allEquipment == null) return;

        foreach (var equipment in allEquipment)
        {
            GameObject slot = GetSlotByType(equipment.Type);
            if (slot != null)
            {
                UpdateSlot(slot, equipment);
            }
        }
    }

    GameObject GetSlotByType(string type)
    {
        switch (type.ToLower())
        {
            case "camera": return cameraSlot;
            case "pc": return pcSlot;
            case "microphone": return microphoneSlot;
            case "light": return lightSlot;
            default: return null;
        }
    }

    void UpdateSlot(GameObject slot, EquipmentData equipment)
    {
        // 슬롯 내부 UI 컴포넌트 (TextMeshProUGUI: 이름, 레벨, 보너스, 비용 / Button: 업그레이드)
        var texts = slot.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 4)
        {
            texts[0].text = GetEquipmentName(equipment.Type);
            texts[1].text = $"Lv. {equipment.Level}";
            texts[2].text = $"보너스: +{equipment.Bonus}";

            long upgradeCost = equipment.Level * 500;
            texts[3].text = $"{upgradeCost:#,0} 골드";
        }

        var button = slot.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnUpgrade(equipment.Type));

            long cost = equipment.Level * 500;
            button.interactable = currentGold >= cost;
        }
    }

    void OnUpgrade(string equipmentType)
    {
        var equipment = System.Array.Find(allEquipment, e => e.Type == equipmentType);
        if (equipment == null) return;

        long cost = equipment.Level * 500;
        if (currentGold < cost)
        {
            SetStatus("골드가 부족합니다.");
            return;
        }

        SetStatus("업그레이드 중...");
        StartCoroutine(APIClient.Instance.UpgradeEquipment(equipmentType, (success, response) =>
        {
            if (success)
            {
                SetStatus($"{GetEquipmentName(equipmentType)} 업그레이드 성공!");
                LoadEquipment(); // 새로고침
            }
            else
            {
                SetStatus("업그레이드 실패.");
            }
        }));
    }

    string GetEquipmentName(string type)
    {
        switch (type.ToLower())
        {
            case "camera": return "카메라";
            case "pc": return "PC";
            case "microphone": return "마이크";
            case "light": return "조명";
            default: return type;
        }
    }

    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    void OnBack()
    {
        gameObject.SetActive(false);
    }
}
