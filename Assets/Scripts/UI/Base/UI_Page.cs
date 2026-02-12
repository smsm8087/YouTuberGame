/// <summary>
/// 탭/페이지 컨텐츠 기본 클래스
/// 팝업 내부의 탭 전환용 (예: 메인 팝업 안의 가챠/캐릭터/장비 탭)
/// </summary>
public abstract class UI_Page : UI_Base
{
    public override void Init()
    {
        base.Init();
    }

    /// <summary>
    /// 페이지 닫힐 때 호출 (정리 로직 오버라이드)
    /// </summary>
    public abstract void Close();
}
