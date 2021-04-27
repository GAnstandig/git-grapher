namespace PrettyGit
{
    public class Range
    {
        public int StartPoint { get; set; }
        public int EndPoint { get; set; }
        public int YOffset { get; set; }

        public Range(int start, int end, int offset)
        {
            StartPoint = start;
            EndPoint = end;
            YOffset = offset;
        }
    }
}
