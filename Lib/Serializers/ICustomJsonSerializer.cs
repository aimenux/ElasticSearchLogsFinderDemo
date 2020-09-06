namespace Lib.Serializers
{
    public interface ICustomJsonSerializer
    {
        string Serialize<T>(T àbj);
    }
}