using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 장비 업그레이드 팝업
/// </summary>
public class EquipmentPopup : UI_Popup
{
    enum Buttons { CloseBtn, UpgradeCameraBtn, UpgradePCBtn, UpgradeMicBtn, UpgradeLightBtn }
    enum Texts
    {
        CameraLevelText, PCLevelText, MicLevelText, LightLevelText,
        CameraCostText, PCCostText, MicCostText, LightCostText,
        GoldText
    }

    protected override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));

        GetButton(Buttons.CloseBtn).AddButtonEvent(() => ClosePop());
        GetButton(Buttons.UpgradeCameraBtn).AddButtonEvent(() => OnUpgrade("Camera"));
        GetButton(Buttons.UpgradePCBtn).AddButtonEvent(() => OnUpgrade("PC"));
        GetButton(Buttons.UpgradeMicBtn).AddButtonEvent(() => OnUpgrade("Microphone"));
        GetButton(Buttons.UpgradeLightBtn).AddButtonEvent(() => OnUpgrade("Light"));
    }

    public override void Init()
    {
        base.Init();
        RefreshUI();
        OpenPop();

        GameObserver.On(ObserverEvent.EquipmentUpgraded, RefreshUI);
        GameObserver.On(ObserverEvent.CurrencyChanged, RefreshUI);
    }

    public override void OnClose()
    {
        base.OnClose();
        GameObserver.Off(ObserverEvent.EquipmentUpgraded, RefreshUI);
        GameObserver.Off(ObserverEvent.CurrencyChanged, RefreshUI);
    }

    void RefreshUI()
    {
        StartCoroutine(APIClient.Instance.GetEquipment((ok, res) =>
        {
            if (!ok) return;
            var data = JsonUtility.FromJson<EquipmentListResponse>(res);
            GetText(Texts.GoldText).text = Util.FormatNumber(data.Gold);

            foreach (var eq in data.Equipment)
            {
                int cost = eq.Level * 500;
                bool isMax = eq.Level >= 10;
                string costStr = isMax ? "MAX" : $"{cost:N0}G";

                switch (eq.Type)
                {
                    case "Camera":
                        GetText(Texts.CameraLevelText).text = $"Lv.{eq.Level}";
                        GetText(Texts.CameraCostText).text = costStr;
                        break;
                    case "PC":
                        GetText(Texts.PCLevelText).text = $"Lv.{eq.Level}";
                        GetText(Texts.PCCostText).text = costStr;
                        break;
                    case "Microphone":
                        GetText(Texts.MicLevelText).text = $"Lv.{eq.Level}";
                        GetText(Texts.MicCostText).text = costStr;
                        break;
                    case "Light":
                        GetText(Texts.LightLevelText).text = $"Lv.{eq.Level}";
                        GetText(Texts.LightCostText).text = costStr;
                        break;
                }
            }
        }));
    }

    void OnUpgrade(string type)
    {
        if (_isTransition) return;
        SetTouchGuard(true);

        StartCoroutine(APIClient.Instance.UpgradeEquipment(type, (ok, res) =>
        {
            SetTouchGuard(false);
            if (!ok) return;
            GameObserver.Emit(ObserverEvent.EquipmentUpgraded);
            GameObserver.Emit(ObserverEvent.CurrencyChanged);
        }));
    }
}
