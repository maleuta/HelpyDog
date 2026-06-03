# HelpyDog
Technologie: ASP.NET Core MVC, Entity Framework Core, SQLite, REST API, Bootstrap, HTML/CSS.

## Opis Projektu
HelpyDog to aplikacja internetowa wspierająca zarządzanie czasem i nawykami (np. nauką, pracą) oparta na mechanizmach grywalizacji. Zamiast klasycznych "list to-do", postępy użytkownika bezpośrednio przekładają się na rozwój jego wirtualnego zwierzaka. Użytkownik logując czas spędzony na produktywnych czynnościach, zdobywa Punkty Doświadczenia (XP), które pozwalają zwierzakowi awansować na kolejne poziomy i za które można kupować przedmioty w wirtualnym sklepie.

### Funkcjonalności (MVC):
* Autoryzacja (Sesje): Dostęp do aplikacji jest chroniony. Logowanie odbywa się z wykorzystaniem mechanizmu sesji. Hasła w bazie danych przechowywane są w postaci bezpiecznych skrótów (hash SHA-256).
* Dashboard (Mój Piesek): Główny panel użytkownika z wizualizacją zwierzaka. Widok dynamicznie generuje obrazki ekwipunku kupionego w sklepie. Posiada mechanikę "spadku szczęścia" (jeśli użytkownik długo się nie uczył, poziom szczęścia zwierzaka maleje).
* Zarządzanie Nawykami (CRUD): Z poziomu interfejsu webowego można dodawać, edytować i usuwać kategorie aktywności wraz z przypisanymi im mnożnikami XP.
* Wirtualny Sklep: Umożliwia wymianę zdobytych punktów XP na przedmioty odnawiające wskaźnik szczęścia.
* Ciekawe Zestawienia: * Ranking: Globalna tablica wyników porównująca poziomy piesków wszystkich zarejestrowanych użytkowników.
* Statystyki tygodnia: Tabela podsumowująca historię nauki z ostatnich 7 dni.
* Panel Administratora: Specjalny widok dostępny tylko dla użytkownika z rolą Admin, służący do tworzenia i przeglądania kont w systemie.

### REST API i Aplikacja Konsolowa
System udostępnia zabezpieczone REST API do logowania aktywności z zewnątrz.
* Uwierzytelnianie następuje poprzez przesłanie w nagłówku żądania (Headers) parametrów: username oraz token (unikalny identyfikator UID przypisany do konta).
* Do projektu dołączono program HelpyDog.ConsoleTimer (aplikacja konsolowa), który symuluje sesję nauki (stoper) i po jej zakończeniu wysyła metodą POST wypracowane minuty do webowej bazy danych.

### Sposób uruchomienia i użycia
1) W głównym katalogu projektu internetowego (HelpyDog.Web) uruchom polecenie dotnet run.

2) Aplikacja automatycznie utworzy plik bazy HelpyDog.db i wygeneruje podstawowe dane (w tym domyślne nawyki, przedmioty w sklepie i konto administratora).

3) Domyślne konto administratora: Login: admin | Hasło: admin123.

4) Aby przetestować zewnętrzne API, należy w osobnej konsoli uruchomić projekt HelpyDog.ConsoleTimer poleceniem dotnet run i postępować zgodnie z instrukcjami na ekranie.
