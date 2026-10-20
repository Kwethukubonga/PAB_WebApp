using PhilisaAbantuBethu.Services;

namespace PhilisaAbantuBethu.Tests;

/// <summary>The same cases as the Android app's PhoneNumberTest, plus the forms people type on a website.</summary>
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
    public void AcceptsRealNumbers(string number) => Assert.True(PhoneNumber.IsValid(number));

    [Theory]
    [InlineData("0888888888")]       // no network uses 088
    [InlineData("085 123 4567")]
    [InlineData("075 123 4567")]
    [InlineData("021 123 4567")]     // landline
    [InlineData("0800 123 456")]     // toll free
    [InlineData("0861 123 456")]     // share call
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
}
