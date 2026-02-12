using System;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 팝업 기본 클래스
/// - 열기/닫기 애니메이션 (DOTween)
/// - 터치가드 (연타 방지)
/// - UIManager 스택 연동
/// </summary>
public abstract class UI_Popup : UI_Base
{
    protected CanvasGroup _canvasGroup;
    protected Transform _panel;
    protected bool _isTransition = false;

    public override void Init()
    {
        base.Init();

        if (_canvasGroup == null)
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
        if (_panel == null)
            _panel = transform;
    }

    /// <summary>
    /// 팝업 열기 애니메이션
    /// </summary>
    protected void OpenPop(Action onComplete = null)
    {
        if (_isTransition) return;
        _isTransition = true;

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
        }

        _panel.localScale = Vector3.zero;
        _panel.DOScale(1f, 0.25f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _isTransition = false;
                onComplete?.Invoke();
            });
    }

    /// <summary>
    /// 팝업 닫기 애니메이션
    /// </summary>
    protected void ClosePop(Action onComplete = null)
    {
        if (_isTransition) return;
        _isTransition = true;

        if (_canvasGroup != null)
            _canvasGroup.DOFade(0f, 0.15f).SetUpdate(true);

        _panel.DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _isTransition = false;
                UIManager.Instance.ClosePopup(this);
                onComplete?.Invoke();
            });
    }

    /// <summary>
    /// 즉시 닫기 (애니메이션 없이)
    /// </summary>
    public void CloseImmediate()
    {
        _panel.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();
        UIManager.Instance.ClosePopup(this);
    }

    /// <summary>
    /// 팝업 정리 (오버라이드해서 코루틴/DOTween 정리)
    /// </summary>
    public virtual void OnClose()
    {
        _panel.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();
    }

    /// <summary>
    /// 터치가드 토글 (애니메이션/서버 요청 중 입력 차단)
    /// </summary>
    protected void SetTouchGuard(bool active)
    {
        if (_canvasGroup != null)
            _canvasGroup.blocksRaycasts = !active || !_isTransition;
    }
}
