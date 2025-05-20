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

