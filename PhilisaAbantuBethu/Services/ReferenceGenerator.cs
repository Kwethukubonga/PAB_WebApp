using System.Security.Cryptography;

namespace PhilisaAbantuBethu.Services;

public interface IReferenceGenerator
{
    string Next();
}

/// <summary>
/// Reference numbers like PAB-7KQ39M. The old PAB + three digits allowed only 900 numbers, so two
/// people would soon be given the same one. Six characters from 31 give about 887 million, and the
/// database's unique index catches the rare clash so a new one is drawn.
///
/// Letters and digits that are easy to mix up when read over the phone (0/O, 1/I/L) are left out.
/// </summary>
public class ReferenceGenerator : IReferenceGenerator
{
    public const string Prefix = "PAB-";
    public const int Length = 6;
    internal const string Alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";

    public string Next() => Prefix + RandomNumberGenerator.GetString(Alphabet, Length);
}
