using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HelpyDog.ConsoleTimer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=====================================");
            Console.WriteLine("      HELPYDOG - ZEWNĘTRZNY KLIENT   ");
            Console.WriteLine("=====================================\n");
            
            Console.Write("Podaj swój login (np. admin): ");
            string username = Console.ReadLine();

            Console.Write("Podaj swój Token API (skopiuj z Panelu Admina na stronie): ");
            string token = Console.ReadLine();

            Console.Write("Wybierz ID Aktywności (np. 1 dla pierwszej na liście): ");
            int habitId = int.Parse(Console.ReadLine() ?? "1");

            // RĘCZNE WPISYWANIE CZASU
            Console.Write("Ile minut chcesz zalogować? Wpisz liczbę (np. 45) i wciśnij ENTER: ");
            string timeInput = Console.ReadLine();
            
            if (!int.TryParse(timeInput, out int durationMinutes) || durationMinutes <= 0)
            {
                Console.WriteLine("Błąd: Wpisano niepoprawny czas. Domyślnie ustawiam na 10 minut.");
                durationMinutes = 10;
            }

            Console.WriteLine($"\nWysyłam zapytanie REST API: {durationMinutes} minut...");
            await SendDataToApi(username, token, habitId, durationMinutes);
        }

        static async Task SendDataToApi(string username, string token, int habitId, int durationMinutes)
        {
            using var client = new HttpClient();
            
            // UWAGA: Upewnij się, że ten adres i port (np. 5100) zgadzają się z Twoją przeglądarką!
            client.BaseAddress = new Uri("http://localhost:5100/"); 
            
            // Dodawanie danych autoryzacyjnych do nagłówków HTTP (Wymóg laboratorium)
            client.DefaultRequestHeaders.Add("username", username);
            client.DefaultRequestHeaders.Add("token", token);

            // Tworzenie "paczki" danych w formacie JSON
            var payload = new
            {
                HabitId = habitId,
                DurationMinutes = durationMinutes
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("api/activities", content);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("SUKCES API: Dane zostały pomyślnie wysłane do Twojego pieska!");
                }
                else
                {
                    Console.WriteLine("BŁĄD API: Autoryzacja odrzucona. Zły login lub Token? Kod błędu: " + response.StatusCode);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("BŁĄD POŁĄCZENIA: Czy strona WWW na pewno jest włączona? Treść: " + e.Message);
            }
            
            Console.WriteLine("\nWciśnij dowolny klawisz, aby zakończyć...");
            Console.ReadLine();
        }
    }
}