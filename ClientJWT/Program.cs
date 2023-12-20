public class Program
{
    public static void Main()
    {
       while (true)
        {
            GetAll();
        }
    }

    static async Task GetAll()
    {
        string apiUrl = "https://localhost:7077/api/Student/GetAll";


        // Token JWT
        Console.Write("Enter your JWT token: ");
        string token = Console.ReadLine();


        using (HttpClient client =  new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            HttpResponseMessage rp = await client.GetAsync(apiUrl);


            if (rp.IsSuccessStatusCode)
            {
                Console.WriteLine("=============================================");
                string content = await rp.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
        }
    }
}