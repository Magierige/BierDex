using Microsoft.AspNetCore.Identity;

namespace BierDex.Models;

public class DutchIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
        => new() { Code = nameof(DefaultError), Description = "Er is een onbekende fout opgetreden." };

    public override IdentityError DuplicateEmail(string email)
        => new() { Code = nameof(DuplicateEmail), Description = $"Het e-mailadres '{email}' is al in gebruik." };

    public override IdentityError DuplicateUserName(string userName)
        => new() { Code = nameof(DuplicateUserName), Description = $"De gebruikersnaam '{userName}' is al in gebruik." };

    public override IdentityError InvalidEmail(string email)
        => new() { Code = nameof(InvalidEmail), Description = $"Het e-mailadres '{email}' is ongeldig." };

    public override IdentityError PasswordTooShort(int length)
        => new() { Code = nameof(PasswordTooShort), Description = $"Het wachtwoord moet minimaal {length} tekens lang zijn." };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        => new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"Het wachtwoord moet minimaal {uniqueChars} unieke tekens bevatten." };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Het wachtwoord moet minimaal één speciaal teken bevatten (bijv. ! of @)." };

    public override IdentityError PasswordRequiresDigit()
        => new() { Code = nameof(PasswordRequiresDigit), Description = "Het wachtwoord moet minimaal één cijfer bevatten ('0'-'9')." };

    public override IdentityError PasswordRequiresLower()
        => new() { Code = nameof(PasswordRequiresLower), Description = "Het wachtwoord moet minimaal één kleine letter bevatten ('a'-'z')." };

    public override IdentityError PasswordRequiresUpper()
        => new() { Code = nameof(PasswordRequiresUpper), Description = "Het wachtwoord moet minimaal één hoofdletter bevatten ('A'-'Z')." };
}