namespace FpgaFlashLoader
{
    public interface ISimpleReadStream
    {
        int Read(byte[] buffer, int offset, int count);
    }
}
