using UnityEngine;

[System.Serializable]
public class Currency
{
    public int solmire;    // Złoto
    public int lunaris;    // Srebro
    public int amberlings; // Brąz

    private const int AmberlingsToLunaris = 100;
    private const int LunarisToSolmire = 1000;

    /// <summary>
    /// Automatycznie konwertuje waluty, jeśli wartości przekraczają limit.
    /// </summary>
    public void Normalize()
    {
        if (amberlings >= AmberlingsToLunaris)
        {
            lunaris += amberlings / AmberlingsToLunaris;
            amberlings %= AmberlingsToLunaris;
        }

        if (lunaris >= LunarisToSolmire)
        {
            solmire += lunaris / LunarisToSolmire;
            lunaris %= LunarisToSolmire;
        }
    }

    /// <summary>
    /// Dodaje określoną ilość Amberlings, automatycznie konwertując je w razie potrzeby.
    /// </summary>
    public void AddAmberlings(int amount)
    {
        amberlings += amount;
        Normalize();
    }

    /// <summary>
    /// Odejmowanie określonej ilości Amberlings (z automatycznym konwertowaniem)
    /// </summary>
    public bool RemoveAmberlings(int amount)
    {
        int totalAmberlings = GetTotalAmberlings();

        if (amount > totalAmberlings) return false; // Brak wystarczającej ilości waluty

        totalAmberlings -= amount;
        SetFromTotalAmberlings(totalAmberlings);

        return true;
    }

    /// <summary>
    /// Zwraca łączną wartość Amberlings (z uwzględnieniem konwersji Lunaris i Solmire).
    /// </summary>
    public int GetTotalAmberlings()
    {
        return amberlings + (lunaris * AmberlingsToLunaris) + (solmire * LunarisToSolmire * AmberlingsToLunaris);
    }

    /// <summary>
    /// Ustawia wartości waluty na podstawie całkowitej liczby Amberlings.
    /// </summary>
    public void SetFromTotalAmberlings(int totalAmberlings)
    {
        solmire = totalAmberlings / (LunarisToSolmire * AmberlingsToLunaris);
        totalAmberlings %= LunarisToSolmire * AmberlingsToLunaris;

        lunaris = totalAmberlings / AmberlingsToLunaris;
        amberlings = totalAmberlings % AmberlingsToLunaris;
    }

    /// <summary>
    /// Zwraca czytelną wersję waluty np. "3 Solmire, 450 Lunaris, 50 Amberlings".
    /// </summary>
    public override string ToString()
    {
        return $"{solmire} Solmire, {lunaris} Lunaris, {amberlings} Amberlings";
    }
}
