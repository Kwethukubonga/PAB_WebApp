using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Tests;

/// <summary>
/// The Android app's PhoneNumberTest cases, plus the forms people type on a website. Unlike the app,
/// the website also takes landlines, so someone with only a landline can ask for help (#27).
/// </summary>
public class PhoneNumberTests
{
    [Theory]
    [InlineData("0821234567")]       // Vodacom
    [InlineData("083 123 4567")]     // MTN
    [InlineData("074 123 4567")]     // Cell C
    [InlineData("081 123 4567")]     // Telkom
    [InlineData("060 123 4567")]     // newer shared block
    [InlineData("071 234 5678")]
    [InlineData("+27 82 123 4567")]
    [InlineData("+27821234567")]
    [InlineData("+27 (0)82 123 4567")] // how the site itself writes numbers
    [InlineData("082-123-4567")]
    [InlineData("+49 151 2345 6789")]  // volunteers and visitors from abroad
    [InlineData("+1 202 555 0143")]
    [InlineData("021 123 4567")]       // Cape Town landline
    [InlineData("011 234 5678")]       // Johannesburg landline
    [InlineData("(021) 765-4321")]
    [InlineData("27821234567")]        // no plus, as copied from WhatsApp
    [InlineData("0027 82 123 4567")]   // 00 instead of +
    [InlineData("0044 20 7946 0958")]
    [InlineData("082.123.4567")]
    public void AcceptsRealNumbers(string number) => Assert.True(PhoneNumber.IsValid(number));

    [Theory]
    [InlineData("0888888888")]       // no network uses 088
    [InlineData("085 123 4567")]
    [InlineData("075 123 4567")]
    [InlineData("0800 123 456")]     // toll free
    [InlineData("0861 123 456")]     // share call
    [InlineData("087 123 4567")]     // internet calling, not a landline or cellphone
    [InlineData("021 111 1111")]     // a made-up landline
    [InlineData("2782123456")]       // 27 with a digit missing
    [InlineData("082 222 2222")]     // same digit repeated
    [InlineData("0710000000")]
    [InlineData("082 123 456")]      // too short
    [InlineData("082 123 45678")]    // too long
    [InlineData("821234567")]        // missing the 0
    [InlineData("phone")]
    [InlineData("")]
    [InlineData(null)]
    public void RefusesNumbersNoOneCouldAnswer(string? number) => Assert.False(PhoneNumber.IsValid(number));

    [Fact]
    public void Tidy_KeepsSingleSpaces() => Assert.Equal("082 123 4567", PhoneNumber.Tidy("  082  123   4567 "));

    [Theory]
    [InlineData("27821234567", "082 123 4567")]
    [InlineData("0027 82 123 4567", "082 123 4567")]
    [InlineData("082.123.4567", "082 123 4567")]
    [InlineData("(021) 765-4321", "021 765 4321")]
    public void Tidy_StoresEverySouthAfricanFormatOneWay(string typed, string stored) =>
        Assert.Equal(stored, PhoneNumber.Tidy(typed));
}
