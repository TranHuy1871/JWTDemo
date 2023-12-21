using JWTFirst.DTOs;
using JWTFirst.Models;
using System.Net;
using System.Net.Http.Json;

public class Program
{
    private static bool _isLoggedIn = false;
    private static TokenModel _tokenModel;

    public static async Task Main()
    {
        while (true)
        {
            await GetStudent();
            await Task.Delay(1000);
        }
    }

    static async Task GetStudent()
    {
        if (!_isLoggedIn)
        {
            // Nếu chưa đăng nhập, thực hiện đăng nhập và lưu trữ token
            _tokenModel = await LoginAndGetTokens();

            if (_tokenModel == null)
            {
                Console.WriteLine("Login failed.");
                return;
            }

            Console.WriteLine("Login successful.");
            _isLoggedIn = true;
        }

        string apiUrl = "https://localhost:7077/api/Student/GetAll";

        // Sử dụng AccessToken để gọi API
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_tokenModel.AccessToken}");
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("=============================================");
                string content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized) // Token hết hạn
            {
                // cập nhật accesstoken bằng cách refresh
                _tokenModel = await RefreshToken(_tokenModel.RefreshToken);

                if (_tokenModel != null)
                {
                    // Lưu trữ accesstoken và refreshtoken mới để sử dụng trong các yêu cầu sau 
                    Console.WriteLine("Token refreshed successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to refresh token. User needs to log in again.");
                    _isLoggedIn = false;
                }
            }
        }
    }


    // login và lấy token
    static async Task<TokenModel> LoginAndGetTokens()
    {
        string loginUrl = "https://localhost:7077/api/Student/Login";
        
        // đang set có 1 tk là string - string
        var studentLoginDTO = new StudentLoginDTO
        {
            UserName = "string",
            Password = "string"
        };

        using (HttpClient client = new HttpClient())
        {
            var response = await client.PostAsJsonAsync(loginUrl, studentLoginDTO);

            // nếu login thành công sẽ return về 1 token
            if (response.IsSuccessStatusCode)
            {
                // lấy token
                var tokenModel = await response.Content.ReadFromJsonAsync<TokenModel>();
                return tokenModel;
            }

            return null;
        }
    }



    //
    static async Task<TokenModel> RefreshToken(string refreshToken)
    {
        string refreshTokenUrl = "https://localhost:7077/api/Student/RefreshToken"; 

        using (HttpClient client = new HttpClient())
        {
            var response = await client.PostAsJsonAsync(refreshTokenUrl, _tokenModel);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TokenModel>();
            }

            return null;
        }
    }
}
