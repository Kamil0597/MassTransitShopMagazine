# MassTransitShopMagazine

# System potwierdzania zamówień

Ten projekt przedstawia logikę działania systemu obsługi zamówień, w którym wymagane są potwierdzenia dostępności produktu od magazynu. Proces uwzględnia równoległe żądania od wielu klientów i pozwala na ich sukces lub porażkę w zależności od decyzji magazynu.

## 📌 Opis ogólny

System składa się z trzech głównych komponentówL
- **Klienci (klient1, klient2) ** - wysyłają żądania zakupu.
- **Sklep** – centralny punkt przetwarzania, który zbiera potwierdzenia.
- **Magazyn** – odpowiada na zapytania o potwierdzenie dostępności produktu (POTW?) zwracając `TAK` lub `NIE`.

## 🔁 Logika działania

1. Klienci (klient1, klient2) wysyłają żądania zakupu (`A`) do sklepu.
2. Sklep dla każdego żądania wysyła zapytanie do magazynu (`POTW?`), pytając o możliwość realizacji.
3. Jeśli dana ilość produktu znajduje się w magazynie, zostaje tymczasowo zablokowana, a magazyn zwraca `TAK`. Sklep przesyła następnie prośbę o potwierdzenie do klienta.
4. Jeśli klient odpowie `TAK`, magazyn wysyła produkt i zamówienie zostaje zrealizowane.
5. Jeśli klient odpowie `NIE`, rezerwacja w magazynie zostaje anulowana, a zamówienie usunięte, produkty odblokowane.

#### 🌀 Saga – zarządzanie cyklem życia zamówienia

System wykorzystuje **Sagę** do obsługi pojedynczego zamówienia klienta. Saga przechowuje informacje o stanie zamówienia (np. `Oczekuje na potwierdzenie`, `Potwierdzone`, `Anulowane`) i odpowiada za koordynację zdarzeń między klientem, sklepem a magazynem.

#### ⏱️ Timeout i automatyczne zakończenie

Każda instancja Sagi ma ustawiony **Timeout** – jeśli klient nie udzieli odpowiedzi w określonym czasie, zamówienie zostaje **automatycznie zakończone** i traktowane jako **anulowane**. Dzięki temu system nie pozostawia wiszących zamówień w nieskończoność.

## 🧩 Schemat procesu realizacji zamówienia

```
               KLIENT
                  |
        żądanie zakupu (A)
                  |
                  V
            +-----------+
            |   SKLEP   |
            +-----------+
                  |
          zapytanie: POTW?
                  |
                  V
           +-------------+
           |  MAGAZYN    |
           +-------------+
                  |
     czy produkt dostępny?
                  |
             TAK → blokuj produkt
                  |
                  V
       sklep pyta klienta o potwierdzenie
                  |
              TAK/NIE
              /     \
             /       \
          TAK        NIE
           |           |
  wysyłka produktu   odblokowanie
     (SUKCES)          (PORAŻKA)
```

### 🎮 Interakcja użytkownika – opis zachowania

| 🧑‍💻 **Rola**   | 🎯 **Czynność**                                                                                  | ⌨️ **Sposób wykonania**            |
|----------------|-----------------------------------------------------------------------------------------------|------------------------------------|
| **Klient**     | Składa zamówienie (ilość produktu)                                                            | Wpisuje liczbę i zatwierdza `Enter` |
| **Klient**     | Potwierdza lub odrzuca zamówienie                                                             | Naciska `T` (tak) lub `N` (nie)     |
| **Magazyn**    | Dodaje nowe sztuki produktu do magazynu                                                       | Wpisuje liczbę i zatwierdza `Enter` |

### ⚙️ Technologie i architektura

Projekt został zbudowany w oparciu o bibliotekę **[MassTransit](https://masstransit-project.com/)** w wersji `6.2.4`, która umożliwia komunikację między komponentami w sposób asynchroniczny oraz zarządzanie stanem poprzez **Sagę**.

### 🚀 Uruchomienie aplikacji lokalnie

Aby uruchomić aplikację na swoim komputerze, należy upewnić się, że masz:

- Zainstalowane środowisko .NET
- Dostęp do instancji RabbitMQ (np. lokalnej lub w chmurze – CloudAMQP)

#### 📝 Konfiguracja RabbitMQ

W pliku startowym aplikacji znajduje się domyślna konfiguracja połączenia z brokerem RabbitMQ w chmurze:

```csharp
var host = cfg.Host(new Uri("rabbitmq://ostrich.lmq.cloudamqp.com/xbgnncht"), h =>
{
    h.Username("xbgnncht");
    h.Password("KCR-knMzxad3qG41vnwjKxnwnD9-Ud8e");
});
```

#### 🔐 **Zamień powyższe dane na własne dane logowania do CloudAMQP **
#### Zmień na login / hasło i URI do twojego CloudAMQP we wszystkich serwisach (Sklep, Magazyn, KlientA, KlientB)

### 🖼️ Przykład działania systemu

Poniżej przedstawiono przykładowy przebieg działania systemu z punktu widzenia klienta i magazynu. Klient wpisuje żądaną liczbę produktów, potwierdza zamówienie, a magazyn odpowiada dostępnością i realizuje wysyłkę.

![image](https://github.com/user-attachments/assets/a5e6dd68-8e02-4bb6-83e5-1fc64c2ee30c)



