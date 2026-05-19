public class RoundScoreResult
{
    public int Score;

    public bool IsFailed;

    public bool IsFatal;

    public string Reason;

    public static RoundScoreResult Success(int score)
    {
        return new RoundScoreResult
        {
            Score = score,
            IsFailed = false,
            IsFatal = false,
            Reason = ""
        };
    }

    public static RoundScoreResult Failed(string reason)
    {
        return new RoundScoreResult
        {
            Score = 0,
            IsFailed = true,
            IsFatal = false,
            Reason = reason
        };
    }

    public static RoundScoreResult Fatal(string reason)
    {
        return new RoundScoreResult
        {
            Score = 0,
            IsFailed = true,
            IsFatal = true,
            Reason = reason
        };
    }
}