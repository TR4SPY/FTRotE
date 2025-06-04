using NUnit.Framework;
using AI_DDA.Assets.Scripts;

public class FormatPlayTimeTests
{
    [TestCase(0f, ExpectedResult = "0s")]
    [TestCase(59f, ExpectedResult = "59s")]
    [TestCase(61f, ExpectedResult = "1m 1s")]
    [TestCase(3661f, ExpectedResult = "1h 1m 1s")]
    [TestCase(90061f, ExpectedResult = "1d 1h 1m 1s")]
    public string FormatPlayTime_ReturnsExpectedString(float seconds)
    {
        return PlayerBehaviorLogger.FormatPlayTime(seconds);
    }
}
