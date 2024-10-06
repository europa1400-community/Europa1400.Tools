namespace Europa1400.Tools.Interfaces
{
    public interface IBaseStruct
    {
        T FromBytes<T>(BinaryReader br) where T : IBaseStruct, new();
    }
}
