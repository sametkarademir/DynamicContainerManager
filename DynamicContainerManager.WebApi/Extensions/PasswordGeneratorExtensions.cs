namespace DynamicContainerManager.WebApi.Extensions;

public static class PasswordGeneratorExtensions
{
    private const string LowerCaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UpperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string NumericChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()-_=+<>?";

    public static string GeneratePassword(this int length, bool includeUpperCase = true, bool includeNumbers = true, bool includeSpecialChars = true)
    {
        var characterSet = LowerCaseChars;

        if (includeUpperCase)
        {
            characterSet += UpperCaseChars;
        }

        if (includeNumbers)
        {
            characterSet += NumericChars;
        }

        if (includeSpecialChars)
        {
            characterSet += SpecialChars;
        }

        var random = new Random();
        var passwordChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            passwordChars[i] = characterSet[random.Next(characterSet.Length)];
        }

        return new string(passwordChars);
    }
}