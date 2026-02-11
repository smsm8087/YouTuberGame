using System;

/// <summary>
/// 인증 API (로그인/회원가입)
/// 성공 시 자동으로 토큰 저장
/// </summary>
public static class AuthApi
{
    public static void Login(string email, string password, Action<bool, AuthResponse> callback)
    {
        var request = new LoginRequest { email = email, password = password };
        ApiClient.Instance.Post<LoginRequest, AuthResponse>("/auth/login", request, (ok, res) =>
        {
            if (ok && res.success)
                ApiClient.Instance.SetAuth(res.token, res.user_id);
            callback?.Invoke(ok && res.success, res);
        });
    }

    public static void Register(string email, string password, string playerName, string channelName, Action<bool, AuthResponse> callback)
    {
        var request = new RegisterRequest
        {
            email = email,
            password = password,
            player_name = playerName,
            channel_name = channelName
        };
        ApiClient.Instance.Post<RegisterRequest, AuthResponse>("/auth/register", request, (ok, res) =>
        {
            if (ok && res.success)
                ApiClient.Instance.SetAuth(res.token, res.user_id);
            callback?.Invoke(ok && res.success, res);
        });
    }

    public static void Logout()
    {
        ApiClient.Instance.ClearAuth();
    }
}
